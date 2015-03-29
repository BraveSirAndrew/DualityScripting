using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Duality;
using Duality.Editor;
using ScriptingPlugin.CSharp;
using ScriptingPlugin.FSharp;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class ScriptReloader
	{
		private static IFileSystem _fileSystem;

		public static void ReloadOutOfDateScripts(IFileSystem fileSystem, IScriptMetadataService metadataService)
		{
			_fileSystem = fileSystem;

			var resources = Resource.GetResourceFiles();

			var csharpScripts = resources.Where(r => r.EndsWith(CSharpScript.FileExt));
			foreach (var csharpScript in csharpScripts)
			{
				ReloadScriptIfOutOfDate(ContentProvider.RequestContent<CSharpScript>(csharpScript), metadataService);
			}

			var fsharpScripts = resources.Where(r => r.EndsWith(FSharpScript.FileExt));
			foreach (var fsharpScript in fsharpScripts)
			{
				ReloadScriptIfOutOfDate(ContentProvider.RequestContent<FSharpScript>(fsharpScript), metadataService);
			}
		}

		private static void ReloadScriptIfOutOfDate<T>(ContentRef<T> script, IScriptMetadataService metadataService) where T : ScriptResourceBase
		{
			var metafilePath = metadataService.GetMetafilePath(script.Path);

			if (string.IsNullOrEmpty(metafilePath))
				return;

			if (string.IsNullOrEmpty(script.Res.SourcePath))
				return;

			if(!_fileSystem.File.Exists(script.Res.SourcePath))
				return;

			if (File.Exists(metafilePath) && File.GetLastWriteTime(script.Res.SourcePath) > File.GetLastWriteTime(metafilePath))
			{
				script.Res.Script = File.ReadAllText(script.Res.SourcePath);
				script.Res.Reload();
				DualityEditorApp.NotifyObjPropChanged(null, new ObjectSelection(script.Res));
			}
		}
	}
}