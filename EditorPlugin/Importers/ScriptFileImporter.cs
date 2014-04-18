using System;
using System.IO;
using Duality;
using Duality.Editor;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor.Importers
{
	public class ScriptFileImporter : IFileImporter
	{
		public bool CanImportFile(string srcFile)
		{
			var ext = Path.GetExtension(srcFile);
			return ext != null && ext.ToLower() == FileConstants.CSharpExtension;
		}

		public void ImportFile(string srcFile, string targetName, string targetDir)
		{
			var res = new ScriptResource
			{
				Script = File.ReadAllText(srcFile),
				SourcePath = srcFile
			};
			res.Save(GetOutputFiles(srcFile, targetName, targetDir)[0]);
		}

		public string[] GetOutputFiles(string srcFile, string targetName, string targetDir)
		{
			var targetResPath = PathHelper.GetFreePath(Path.Combine(targetDir, targetName), ScriptResource.FileExt);
			return new[] { targetResPath };
		}

		public bool IsUsingSrcFile(ContentRef<Resource> r, string srcFile)
		{
			var res = r.As<ScriptResource>();
			return res != null && res.Res.SourcePath == srcFile;
		}

		public void ReimportFile(ContentRef<Resource> r, string srcFile)
		{
			var res = r.Res as ScriptResource;

			if (res == null)
				throw new ArgumentException(string.Format("ScriptFileImporter was used to import a resource of type '{0}'.", r.ResType.Name));
            
            res.Script = File.ReadAllText(srcFile);
			res.Reload();
		}
	}
}