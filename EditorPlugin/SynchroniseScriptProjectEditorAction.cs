using System.Collections;
using System.IO;
using System.Windows.Forms;
using Duality;
using Duality.Editor;
using Duality.Editor.Forms;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class SynchroniseScriptProjectEditorAction : EditorSingleAction<ScriptResourceBase>
	{
		public override string Name
		{
			get { return "Synchronise Visual Studio project"; }
		}

		public override string Description
		{
			get { return "Synchronises the script project with the scripts in Duality by regenerating the project and saving all scripts to the source/media folder"; }
		}

		public override void Perform(ScriptResourceBase resource)
		{
			var result = MessageBox.Show("This action will overwrite any unsaved changes to the scripts in your Source\\Media directory. Are you sure you want to continue?",
				"Synchronise with Visual Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (result == DialogResult.No)
				return;

			var processDialog = new ProcessingBigTaskDialog("Synchronising scripts", "Making your script project pretty again. This shouldn't take long", SynchroniseScriptProjects, null)
			{
				MainThreadRequired = false
			};
			processDialog.ShowDialog(DualityEditorApp.MainForm);
		}

		public override bool MatchesContext(string context)
		{
			return context == DualityEditorApp.ActionContextMenu;
		}

		private static IEnumerable SynchroniseScriptProjects(ProcessingBigTaskDialog.WorkerInterface worker)
		{
			var projectEditor = new ScriptProjectEditor();
			projectEditor.RemoveAllScriptReferences(ScriptingEditorPlugin.CSharpProjectPath);
			projectEditor.RemoveAllScriptReferences(ScriptingEditorPlugin.FSharpProjectPath);

			ScriptingEditorPlugin.AddScriptProjectsToSolution();

			var scriptResources = Resource.GetResourceFiles("Data\\Scripts");
			var totalFiles = scriptResources.Count;
			var filesProcessed = 0f;

			foreach (var script in scriptResources)
			{
				var contentRef = ContentProvider.RequestContent<ScriptResourceBase>(script);

				string projectPath;
				string extension;
				if (contentRef.Res is CSharpScript)
				{
					projectPath = ScriptingEditorPlugin.CSharpProjectPath;
					extension = ".cs";
				}
				else
				{
					projectPath = ScriptingEditorPlugin.FSharpProjectPath;
					extension = ".fs";
				}

				var sourceFilePath = contentRef.Res.SourcePath;
				if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
				{
					sourceFilePath = FileImportProvider.GenerateSourceFilePath(contentRef.As<Resource>(), extension);
					Directory.CreateDirectory(Path.GetDirectoryName(sourceFilePath));
					contentRef.Res.SourcePath = sourceFilePath;
				}

				if (sourceFilePath != null)
					contentRef.Res.SaveScript(sourceFilePath);

				sourceFilePath = sourceFilePath.Replace("Source\\Media", ProjectConstants.MediaFolder);
				var scriptFileName = Path.GetFileName(sourceFilePath);

				projectEditor.AddScriptToProject(sourceFilePath, scriptFileName, projectPath);

				worker.Progress = filesProcessed++ / totalFiles;
				
				yield return null;
			}
		}
	}
}