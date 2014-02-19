using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Duality;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	public class ScriptResource : Resource
	{
		public new const string FileExt = ".cs" + Resource.FileExt;

		[field: NonSerialized]
		public event EventHandler Reloaded;

		[NonSerialized]
		private Assembly _assembly;

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
			if (File.Exists("Scripts\\Scripts.dll"))
			{
				_assembly = Assembly.LoadFile(System.IO.Path.GetFullPath("Scripts\\Scripts.dll"));
			}
			else
			{
				_assembly = ScriptingPluginCorePlugin.ScriptCompiler.Compile(Name, SourcePath, Script);
			}
		}

		public DualityScript Instantiate()
		{
			if (_assembly == null)
				return null;

			var script = _assembly.GetTypes().FirstOrDefault(t => t.BaseType != null && t.BaseType == typeof(DualityScript));

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
