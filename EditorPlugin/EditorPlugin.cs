using System;
using System.IO;
using System.Linq;
using Duality;
using DualityEditor;
using DualityEditor.CorePluginInterface;
using DualityEditor.Forms;
using Ionic.Zip;
using ScriptingPlugin.Editor.Importers;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
    public class ScriptingEditorPlugin : EditorPlugin
	{
	    private const string SolutionProjectReferences = "\nProject(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"Scripts\", \"Scripts\\Scripts.csproj\", \"{1DC301F5-644D-4109-96C4-2158ABDED70D}\"\nEndProject";

		private bool _debuggerAttachedLastFrame;

	    public override string Id
		{
			get { return "ScriptingEditorPlugin"; }
		}

	    protected override void LoadPlugin()
	    {
		    base.LoadPlugin();

			CorePluginRegistry.RegisterFileImporter(new ScriptFileImporter());
			CorePluginRegistry.RegisterFileImporter(new FSharpScriptFileImporter());

			CorePluginRegistry.RegisterTypeCategory(typeof(ScriptResource), "Scripting");
			CorePluginRegistry.RegisterTypeCategory(typeof(FSharpScript), "Scripting");

			CorePluginRegistry.RegisterEditorAction(new EditorAction<ScriptResource>(null, null, ActionOpenScriptFile, "Open C# script file"), CorePluginRegistry.ActionContext_OpenRes);
			
			CorePluginRegistry.RegisterEditorAction(new EditorAction<FSharpScript>(null, null, ActionOpenFSharpScriptFile, "Open F# script file"), CorePluginRegistry.ActionContext_OpenRes);
	    }

		protected override void InitPlugin(MainForm main)
		{
			base.InitPlugin(main);

			ReloadOutOfDateScripts();

			FileEventManager.ResourceCreated += OnResourceCreated;

			ModifySolution();

			DualityEditorApp.Idling += DualityEditorAppOnIdling;
		}

	    private void DualityEditorAppOnIdling(object sender, EventArgs eventArgs)
	    {
			if (System.Diagnostics.Debugger.IsAttached && _debuggerAttachedLastFrame == false)
			{
				foreach (var script in ContentProvider.GetAvailableContent<ScriptResource>())
				{
					script.Res.Reload();
				}

				_debuggerAttachedLastFrame = true;
			}
			else if (System.Diagnostics.Debugger.IsAttached == false && _debuggerAttachedLastFrame)
			{
				_debuggerAttachedLastFrame = false;
			}
	    }

	    private static void ModifySolution()
	    {
		    if (File.Exists(Path.Combine(EditorHelper.SourceCodeDirectory, "Scripts" + Path.DirectorySeparatorChar + "Scripts.csproj")))
			    return;

		    ExtractScriptProjectToCodeDirectory();
		    AddScriptProjectToSolution();
	    }

	    private void ReloadOutOfDateScripts()
	    {
		    foreach (var script in ContentProvider.GetAvailableContent<ScriptResource>())
		    {
				var metafilePath = Path.GetFullPath(script.Res.GetMetafilePath());

			    if (string.IsNullOrEmpty(metafilePath))
				    continue;

			    if (string.IsNullOrEmpty(script.Res.SourcePath))
				    continue;

				if (File.Exists(metafilePath) && File.GetLastWriteTime(script.Res.SourcePath) > File.GetLastWriteTime(metafilePath))
				{
					script.Res.Script = File.ReadAllText(script.Res.SourcePath);
					script.Res.Reload();
					DualityEditorApp.NotifyObjPropChanged(null, new ObjectSelection(script.Res));
				}
		    }
	    }

	    private static void AddScriptProjectToSolution()
	    {
		    var slnPath = Directory.GetFiles(EditorHelper.SourceCodeDirectory, "*.sln").First();
		    var slnText = File.ReadAllText(slnPath);
		    slnText = slnText.Insert(slnText.LastIndexOf("EndProject", StringComparison.OrdinalIgnoreCase) + 10,
			    SolutionProjectReferences);
		    File.WriteAllText(slnPath, slnText);
	    }

	    private static void ExtractScriptProjectToCodeDirectory()
	    {
		    using (var scriptsProjectZip = ZipFile.Read(Resources.Resources.ScriptsProjectTemplate))
		    {
			    scriptsProjectZip.ExtractAll(Path.Combine(EditorHelper.SourceCodeDirectory, "Scripts"),
				    ExtractExistingFileAction.DoNotOverwrite);
		    }
	    }

	    private void OnResourceCreated(object sender, ResourceEventArgs e)
	    {
		    if (e.ContentType == typeof (ScriptResource))
		    {
			    var script = e.Content.As<ScriptResource>();
			    script.Res.Script = Resources.Resources.ScriptTemplate;
			    script.Res.Save();
		    }
			else if (e.ContentType == typeof(FSharpScript))
		    {
				var script = e.Content.As<FSharpScript>();
				script.Res.Script = Resources.Resources.FSharpScriptTemplate;
				script.Res.Save();
		    }
	    }

	    private static void ActionOpenScriptFile(ScriptResource script)
	    {
			if (script == null) 
				return;

			FileImportProvider.OpenSourceFile(script, FileConstants.CSharpExtension, script.SaveScript);
	    }

	    private static void ActionOpenFSharpScriptFile(FSharpScript script)
	    {
			if (script == null) 
				return;

			FileImportProvider.OpenSourceFile(script, ".fs", script.SaveScript);
	    }
	}
}
