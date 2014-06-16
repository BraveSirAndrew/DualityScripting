using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Duality;
using Duality.Editor;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	public abstract class ScriptResourceBase : Resource
	{
		[field: NonSerialized]
		public event EventHandler Reloaded;

		private Assembly _assembly;
		[NonSerialized]
		private ScriptCompilerResult _scriptCompilerResult;
		[NonSerialized]
		protected IScriptCompilerService ScriptCompiler;

		[EditorHintFlags(MemberFlags.Invisible)]
		public Assembly Assembly
		{
			get
			{
				if (_assembly == null)
				{
					Compile();

					if(_scriptCompilerResult != null)
						_assembly = _scriptCompilerResult.Assembly;
				}

				return _assembly;
			}
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
			const string scriptsDll = "Scripts\\Scripts.dll";
			if (File.Exists(scriptsDll))
			{
				_assembly = Assembly.LoadFile(System.IO.Path.GetFullPath(scriptsDll));
				return;
			}
			try
			{
				if (!string.IsNullOrEmpty(SourcePath))
				{
					_scriptCompilerResult = ScriptCompiler.TryCompile(Name, SourcePath, Script);
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
			if (Assembly == null || _scriptCompilerResult == null || _scriptCompilerResult.CompilerResult != CompilerResult.AssemblyExists)
			{
				Log.Editor.WriteWarning("Couldn't compile script '{0}'", Name);
				return null;
			}

			var script = Assembly.GetTypes().FirstOrDefault(t => t.BaseType != null && t.BaseType == typeof(DualityScript) && t.Name == Name);

			if (script == null)
			{
				Log.Game.WriteError("Could not create an instance of script '{0}' because it does not contain a type derived from DualityScript.", Name);
				return null;
			}

			return (DualityScript)Activator.CreateInstance(script);
		}

		public void Reload()
		{
			Compile();

			var metafilePath = System.IO.Path.GetFullPath(GetMetafilePath());

			if (File.Exists(metafilePath))
			{
				var fileInfo = new FileInfo(metafilePath);
				fileInfo.Attributes &= ~FileAttributes.Hidden;
			}

			File.WriteAllText(metafilePath, "");
			File.SetLastWriteTime(metafilePath, DateTime.Now);
			File.SetAttributes(metafilePath, FileAttributes.Hidden);

			OnReloaded();
		}

		public string GetMetafilePath()
		{
			return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), System.IO.Path.GetFileNameWithoutExtension(Path) + ".meta");
		}

		protected virtual void OnReloaded()
		{
			var handler = Reloaded;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}
	}
}