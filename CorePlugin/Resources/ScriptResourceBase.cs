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
#if !DEBUG
			var scriptsDll = new[] { "Scripts\\Scripts.dll", "Scripts\\FSharpScripts.dll" };
			foreach (string script in scriptsDll)
			{
				if (!File.Exists(script)) 
					continue;
				_assembly = Assembly.LoadFile(System.IO.Path.GetFullPath(script));
				return;
			}
#endif
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
			if (Assembly == null)
			{
				Compile();

				if (_scriptCompilerResult.CompilerResult != CompilerResult.AssemblyExists)
					return null;
			}

			var scriptType = _assembly.GetTypes().FirstOrDefault(t => t.BaseType != null && t.BaseType == typeof(DualityScript));

			if (scriptType == null)
			{
				Log.Game.WriteError("Could not create an instance of script '{0}' because it does not contain a type derived from DualityScript.", Name);
				return null;
			}

			if (scriptType.Name != Name)
			{
				Log.Game.WriteError("Could not create an instance of script '{0}' because the class name is '{1}' and should be '{0}'", Name, scriptType.Name);
				return null;
			}

			return (DualityScript)Activator.CreateInstance(scriptType);
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