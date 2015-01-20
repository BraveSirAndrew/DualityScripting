using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Duality;
using Duality.Editor;

namespace ScriptingPlugin.Resources
{
    [Serializable]
    [ExplicitResourceReference(new Type[0])]
    public abstract class ScriptResourceBase : Resource
    {
        [field: NonSerialized]
        public event EventHandler Reloaded;

        [NonSerialized]
        private Assembly _assembly;
        [NonSerialized]
        private ScriptCompilerResult _scriptCompilerResult;
        [NonSerialized]
        protected IScriptCompilerService ScriptCompiler;
        [NonSerialized]
        protected IScriptMetadataService ScriptMetadataService;

        public ScriptResourceBase()
        {
            ScriptMetadataService = ScriptingPluginCorePlugin.ScriptMetadataService;
        }

        [EditorHintFlags(MemberFlags.Invisible)]
        public Assembly Assembly
        {
            get { return _assembly; }
        }

        public string Script { get; set; }

        public void SaveScript(string scriptPath)
        {
            if (scriptPath == null)
                scriptPath = sourcePath;

            if (!IsDefaultContent && sourcePath == null)
                sourcePath = scriptPath;

            File.WriteAllText(sourcePath, Script);
        }

        private void Compile()
        {
            try
            {
                if (!string.IsNullOrEmpty(SourcePath))
                {
                    _scriptCompilerResult = ScriptCompiler.TryCompile(Name, SourcePath, Script);

                    if (_scriptCompilerResult != null && _scriptCompilerResult.CompilerResult == CompilerResult.AssemblyExists)
                        _assembly = _scriptCompilerResult.Assembly;

                    return;
                }
            }
            catch (Exception e)
            {
                Log.Editor.WriteError("Error trying to compile script {0}.Message {1} \n {2}", Name, e.Message, e.StackTrace);
            }

            Log.Editor.WriteWarning("The script resource '{0}' has no SourcePath and can't be compiled.", Name);
        }

        public DualityScript Instantiate()
        {
            Type scriptType = null;

            var assemblies = PrebuildScripts.LoadAssemblies();
            if (assemblies.Any())
            {
	            foreach (Type type in assemblies.Select(assembly => FindTypeInAssembly(assembly, Name)).Where(type => type != null))
	            {
		            scriptType = type;
		            break;
	            }
            }
            
            if (scriptType == null)
            {
				if(_assembly == null)
					Compile();

                scriptType = FindTypeInAssembly(_assembly, Name);
                if (_scriptCompilerResult != null && _scriptCompilerResult.CompilerResult != CompilerResult.AssemblyExists)
                    return null;
            }

            if (scriptType == null)
            {
                Log.Game.WriteError("Could not create an instance of script '{0}' because it does not contain a type derived from DualityScript.", Name);
                return null;
            }

            if (scriptType.Name != Name)
            {
                Log.Game.WriteError("Could not create an instance of script '{0}'.Possibly because the class name is '{1}' and should be '{0}'", Name, scriptType.Name);
                return null;
            }

            return (DualityScript)Activator.CreateInstance(scriptType);
        }

        private Type FindTypeInAssembly(Assembly assembly, string typeName)
        {
            Type type = null;
            if (assembly == null)
                return null;
            type = assembly.GetTypes().FirstOrDefault(t =>
                t.BaseType != null &&
                t.BaseType == typeof(DualityScript) &&
                String.Equals(t.Name, typeName, StringComparison.CurrentCultureIgnoreCase));
            return type;
        }
		
        public void Reload()
        {
            Compile();

            if (ScriptMetadataService == null)
            {
                Log.Editor.WriteError("The script metadata service hasn't been set up. Can't reload script '{0}'.", Name);
                return;
            }

            ScriptMetadataService.UpdateMetadata(Path);

            OnReloaded();
        }

        protected virtual void OnReloaded()
        {
            var handler = Reloaded;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}