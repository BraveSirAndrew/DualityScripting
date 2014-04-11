using System;
using System.IO;
using System.Linq;
using Duality;
using Duality.Editor;
using Duality.Editor.Forms;
using Duality.Resources;
using Ionic.Zip;
using Microsoft.Build.Construction;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class ScriptingEditorPlugin : EditorPlugin
	{
		
	    private const string SolutionProjectReferences = "\nProject(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"Scripts\", \"Scripts\\Scripts.csproj\", \"{1DC301F5-644D-4109-96C4-2158ABDED70D}\"\nEndProject";

		private bool _debuggerAttachedLastFrame;
		private static string _scriptsProjectPath;

		private const string Scripts = "Scripts";

	    public override string Id
		{
			get { return "ScriptingEditorPlugin"; }
		}
		
		protected override void InitPlugin(MainForm main)
		{
			base.InitPlugin(main);

			ReloadOutOfDateScripts();

			FileEventManager.ResourceCreated += OnResourceCreated;
			FileEventManager.ResourceRenamed += OnResourceRenamed;

			ModifySolution();

			DualityEditorApp.EditorIdling += DualityEditorAppOnIdling;
			_scriptsProjectPath = Path.Combine(EditorHelper.SourceCodeDirectory, Scripts, Scripts + ".csproj");
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
		    
		    if (File.Exists(_scriptsProjectPath))
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
			    scriptsProjectZip.ExtractAll(Path.Combine(EditorHelper.SourceCodeDirectory, Scripts),
				    ExtractExistingFileAction.DoNotOverwrite);
		    }
	    }

		private void OnResourceRenamed(object sender, ResourceRenamedEventArgs e)
		{
			if (e.ContentType != typeof(ScriptResource))
				return;
			RemoveOldScriptFromProject(e.OldContent.FullName);

			var newScriptName = GetScriptNameWithPath(RemoveDataScriptPath(e.Content.FullName));
			AddScriptToSolution(newScriptName, GetFileName(newScriptName));
		}

		private void RemoveOldScriptFromProject(string oldContentName)
		{
			ProjectRootElement rootElement = ProjectRootElement.Open(Path.Combine(Duality.PathHelper.ExecutingAssemblyDir, _scriptsProjectPath));
			if (rootElement == null)
				return;
			
			string scriptName = GetScriptNameWithPath(RemoveDataScriptPath(oldContentName));

			foreach (var itemGroup in rootElement.ItemGroups)
			{
				foreach (ProjectItemElement item in itemGroup.Items)
				{
					if (item.Include.Contains(scriptName))
					{
						rootElement.RemoveChild(itemGroup);
					}
				}
			}
			rootElement.Save();
		}

		private void OnResourceCreated(object sender, ResourceEventArgs e)
	    {
		    if (e.ContentType != typeof (ScriptResource))
			    return;

		    var script = e.Content.As<ScriptResource>();
		    script.Res.Script = Resources.Resources.ScriptTemplate;
			script.Res.Save();

		    var fileWithPath = RemoveDataScriptPath(script.FullName);
			AddScriptToSolution(GetScriptNameWithPath(fileWithPath), GetFileName(fileWithPath));
	    }

		private string GetFileName(string fileWithPath)
		{
			return Path.GetFileName(fileWithPath);
		}

		public string RemoveDataScriptPath(string fullScriptName)
		{
			return fullScriptName.Replace(ScriptingPluginCorePlugin.DataScripts, string.Empty) + ScriptingPluginCorePlugin.CSharpScriptExtension;
		}

		public string GetScriptNameWithPath(string fileNameWithResourcePath)
		{
			return Path.Combine(@"..\..\Media", Scripts, fileNameWithResourcePath);
		}

		private void AddScriptToSolution(string scriptPath, string scriptFileName)
		{
			try
			{
				var rootElement = ProjectRootElement.Open(Path.Combine(Duality.PathHelper.ExecutingAssemblyDir, _scriptsProjectPath));
				if (rootElement == null)
					return;
				var itemGroup = rootElement.AddItemGroup();


				var itemElement = itemGroup.AddItem("compile", scriptPath);
				itemElement.AddMetadata("link", scriptFileName);
				rootElement.Save();
			}
			catch (Exception exception)
			{
				Log.Editor.WriteError("There was a problem editing the Scripts project. The error is {0} \n StackTrace: {1}",exception.Message, exception.StackTrace);
				
			}
			
		}
	}
}
