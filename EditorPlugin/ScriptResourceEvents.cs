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
				var scriptFullName = SaveResource<ScriptResource>(e.Content, Resources.Resources.ScriptTemplate);

				var fileWithPath = RemoveDataScriptPath(scriptFullName, ScriptingPluginCorePlugin.CSharpScriptExtension);
				AddScriptToSolution(GetScriptNameWithPath(fileWithPath), GetFileName(fileWithPath));
			}
			else if (e.ContentType == typeof(FSharpScript))
				SaveResource<FSharpScript>(e.Content, Resources.Resources.FSharpScriptTemplate);
		}

		private string SaveResource<T>(ContentRef<Resource> scriptResourceContentRef, string script) where T : ScriptResourceBase
		{
			var scriptResource = scriptResourceContentRef.As<T>();

			scriptResource.Res.Script = script;
			scriptResource.Res.Save();
			return scriptResource.FullName;
		}

		public void OnResourceRenamed(object sender, ResourceRenamedEventArgs e)
		{
			string extension = null;
			if (!e.ContentType.IsAssignableFrom(typeof(ScriptResourceBase)))
				return;
			if (e.ContentType == typeof(ScriptResource))
			{
				extension = ScriptingPluginCorePlugin.CSharpScriptExtension;
			}
			else if (e.ContentType == typeof(FSharpScript))
			{
				extension = ScriptingPluginCorePlugin.FSharpScriptExtension;
			}
			var oldName = e.OldContent.FullName;
			RemoveOldScriptFromProject(oldName, extension);
			var newScriptName = GetScriptNameWithPath(RemoveDataScriptPath(e.Content.FullName, extension));
			AddScriptToSolution(newScriptName, GetFileName(newScriptName));
		}

		private void RemoveOldScriptFromProject(string oldContentName, string extension)
		{
			var rootElement = ProjectRootElement.Open(Path.Combine(PathHelper.ExecutingAssemblyDir, ScriptingEditorPlugin.ScriptsProjectPath));
			if (rootElement == null)
				return;

			string scriptName = GetScriptNameWithPath(RemoveDataScriptPath(oldContentName, extension));

			foreach (var itemGroup in rootElement.ItemGroups)
			{
				foreach (var item in itemGroup.Items)
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

		private string RemoveDataScriptPath(string fullScriptName, string extension)
		{
			return fullScriptName.Replace(ScriptingPluginCorePlugin.DataScripts, string.Empty) + extension;
		}

		private void AddScriptToSolution(string scriptPath, string scriptFileName)
		{
			try
			{
				var rootElement = ProjectRootElement.Open(Path.Combine(PathHelper.ExecutingAssemblyDir, ScriptingEditorPlugin.ScriptsProjectPath));
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