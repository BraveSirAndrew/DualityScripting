using System.IO;
using System.IO.Abstractions;
using Duality;
using Duality.Editor;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class ScriptReloader
	{
		private static IFileSystem _fileSystem;

		public static void ReloadOutOfDateScripts(IFileSystem fileSystem, IScriptMetadataService metadataService)
		{
			_fileSystem = fileSystem;

			foreach (var script in ContentProvider.GetAvailableContent<CSharpScript>())
				ReloadScript(script, metadataService);
			foreach (var script in ContentProvider.GetAvailableContent<FSharpScript>())
				ReloadScript(script, metadataService);
		}

		private static void ReloadScript<T>(ContentRef<T> script, IScriptMetadataService metadataService) where T : ScriptResourceBase
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