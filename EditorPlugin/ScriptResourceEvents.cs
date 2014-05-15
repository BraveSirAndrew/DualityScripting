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
		private const string Scripts = "Scipts";
		public void OnResourceCreated(object sender, ResourceEventArgs e)
		{
			string scriptFullName = null;
			string scriptExtension = null;
			if (e.ContentType == typeof(CSharpScript))
			{
				scriptFullName = SaveResource<CSharpScript>(e.Content, Resources.Resources.ScriptTemplate);
				scriptExtension = ScriptingPluginCorePlugin.CSharpScriptExtension;
			}
			else if (e.ContentType == typeof (FSharpScript))
			{
				scriptFullName= SaveResource<FSharpScript>(e.Content, Resources.Resources.FSharpScriptTemplate);
				scriptExtension = ScriptingPluginCorePlugin.FSharpScriptExtension;
			}
			var fileWithPath = RemoveDataScriptPath(scriptFullName, scriptExtension);
			AddScriptToSolution(GetScriptNameWithPath(fileWithPath), GetFileName(fileWithPath), ScriptingEditorPlugin.CSharpProjectPath);
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
			string projectPath = null;
			if (!(typeof(ScriptResourceBase)).IsAssignableFrom(e.ContentType))
				return;
			if (e.ContentType == typeof(CSharpScript))
			{
				extension = ScriptingPluginCorePlugin.CSharpScriptExtension;
				projectPath = ScriptingEditorPlugin.CSharpProjectPath;
			}
			else if (e.ContentType == typeof(FSharpScript))
			{
				extension = ScriptingPluginCorePlugin.FSharpScriptExtension;
				projectPath = ScriptingEditorPlugin.FSharpProjectPath;
			}
			var oldName = e.OldContent.FullName;
			RemoveOldScriptFromProject(oldName, extension, projectPath);
			var newScriptName = GetScriptNameWithPath(RemoveDataScriptPath(e.Content.FullName, extension));
			AddScriptToSolution(newScriptName, GetFileName(newScriptName), projectPath);
		}

		private void RemoveOldScriptFromProject(string oldContentName, string extension, string projectPath)
		{
			var rootElement = ProjectRootElement.Open(Path.Combine(PathHelper.ExecutingAssemblyDir, projectPath));
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
			return Path.Combine(@"..\..\Media", Scripts, fileNameWithResourcePath);
		}

		private string RemoveDataScriptPath(string fullScriptName, string extension)
		{
			return fullScriptName.Replace(ScriptingPluginCorePlugin.DataScripts, string.Empty) + extension;
		}

		private void AddScriptToSolution(string scriptPath, string scriptFileName, string projectPath)
		{
			try
			{
				var rootElement = ProjectRootElement.Open(Path.Combine(PathHelper.ExecutingAssemblyDir, projectPath));
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