using System;
using System.IO;
using Duality;
using Duality.Editor;
using Microsoft.Build.Construction;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class ScriptResourceEvents
	{
		public void OnResourceCreated(object sender, ResourceEventArgs e)
		{
			if (e.ContentType == typeof(ScriptResource))
			{
				var script = e.Content.As<ScriptResource>();
				script.Res.Script = Resources.Resources.ScriptTemplate;
				script.Res.Save();

				var fileWithPath = RemoveDataScriptPath(script.FullName);
				AddScriptToSolution(GetScriptNameWithPath(fileWithPath), GetFileName(fileWithPath));
			}
			else if (e.ContentType == typeof(FSharpScript))
			{
				var script = e.Content.As<FSharpScript>();
				script.Res.Script = Resources.Resources.FSharpScriptTemplate;
				script.Res.Save();
			}
		}

		public void OnResourceRenamed(object sender, ResourceRenamedEventArgs e)
		{
			if (e.ContentType != typeof(ScriptResource))
				return;
			RemoveOldScriptFromProject(e.OldContent.FullName);

			var newScriptName = GetScriptNameWithPath(RemoveDataScriptPath(e.Content.FullName));
			AddScriptToSolution(newScriptName, GetFileName(newScriptName));
		}

		private void RemoveOldScriptFromProject(string oldContentName)
		{
			var rootElement = ProjectRootElement.Open(Path.Combine(Duality.PathHelper.ExecutingAssemblyDir, ScriptingEditorPlugin.ScriptsProjectPath));
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


		private string GetFileName(string fileWithPath)
		{
			return Path.GetFileName(fileWithPath);
		}

		private string GetScriptNameWithPath(string fileNameWithResourcePath)
		{
			return Path.Combine(@"..\..\Media", ScriptingEditorPlugin.Scripts, fileNameWithResourcePath);
		}

		private string RemoveDataScriptPath(string fullScriptName)
		{
			return fullScriptName.Replace(ScriptingPluginCorePlugin.DataScripts, string.Empty) + ScriptingPluginCorePlugin.CSharpScriptExtension;
		}

		private void AddScriptToSolution(string scriptPath, string scriptFileName)
		{
			try
			{
				var rootElement = ProjectRootElement.Open(Path.Combine(Duality.PathHelper.ExecutingAssemblyDir, ScriptingEditorPlugin.ScriptsProjectPath));
				if (rootElement == null)
					return;
				var itemGroup = rootElement.AddItemGroup();


				var itemElement = itemGroup.AddItem("compile", scriptPath);
				itemElement.AddMetadata("link", scriptFileName);
				rootElement.Save();
			}
			catch (Exception exception)
			{
				Log.Editor.WriteError("There was a problem editing the Scripts project. The error is {0} \n StackTrace: {1}", exception.Message, exception.StackTrace);

			}
		}
	}
}