using System;
using System.IO;
using Duality;
using Duality.Editor;
using ScriptingPlugin.CSharp;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor.Importers
{
	public abstract class ScriptFileImporter<T> : IFileImporter where T : ScriptResourceBase, new()
	{
		public bool CanImportFile(string srcFile)
		{
			var ext = Path.GetExtension(srcFile);
			return ext != null && ext.ToLower() == DefaultFileExtention();
		}

		public void ImportFile(string srcFile, string targetName, string targetDir)
		{
			var res = new T
			{
				Script = File.ReadAllText(srcFile),
				SourcePath = srcFile
			};
			res.Save(GetOutputFiles(srcFile, targetName, targetDir)[0]);
		}

		public string[] GetOutputFiles(string srcFile, string targetName, string targetDir)
		{
			var targetResPath = PathHelper.GetFreePath(Path.Combine(targetDir, targetName), DefaultFileExtention());
			return new[] { targetResPath };
		}

		protected abstract string DefaultFileExtention();

		public bool IsUsingSrcFile(ContentRef<Resource> r, string srcFile)
		{
			var res = r.As<T>();
			return res != null && res.Res.SourcePath == srcFile;
		}

		public void ReimportFile(ContentRef<Resource> r, string srcFile)
		{
			var res = r.Res as T;

			if (res == null)
				throw new ArgumentException(string.Format("ScriptFileImporter was used to import a resource of type '{0}'.", r.ResType.Name));
            
            res.Script = File.ReadAllText(srcFile);
			res.Reload();
		}
	}

	public class CSharpFileImporter : ScriptFileImporter<CSharpScript>
	{
		protected override string DefaultFileExtention()
		{
			return FileConstants.CSharpExtension;
		}
	}

	public class FSharpFileImporter : ScriptFileImporter<FSharpScript>
	{
		protected override string DefaultFileExtention()
		{
			return FileConstants.FSharpExtension;
		}
	}
}