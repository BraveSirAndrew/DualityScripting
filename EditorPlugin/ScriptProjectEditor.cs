using System;
using System.IO;
using Duality;
using Microsoft.Build.Construction;

namespace ScriptingPlugin.Editor
{

	public interface IScriptProjectEditor
	{
		void AddScriptToProject(string scriptPath, string scriptFileName, string projectPath);
		void RemoveOldScriptFromProject(string oldContentName, string extension, string projectPath);
	}
	public class ScriptProjectEditor : IScriptProjectEditor
	{
		private const string Scripts = "Scripts";

		public void AddScriptToProject(string scriptPath, string scriptFileName, string projectPath)
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

		public void RemoveOldScriptFromProject(string oldContentName, string extension, string projectPath)
		{
			var rootElement = ProjectRootElement.Open(projectPath);
			if (rootElement == null)
				return;

			string fileNameWithResourcePath = RemoveDataScriptPath(oldContentName, extension);
			if (string.IsNullOrWhiteSpace(fileNameWithResourcePath))
				return;
			string scriptName = GetScriptNameWithPath(fileNameWithResourcePath);

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

		private string GetScriptNameWithPath(string fileNameWithResourcePath)
		{
			return Path.Combine(@"..\..\Media", Scripts, fileNameWithResourcePath);
		}

		private string RemoveDataScriptPath(string fullScriptName, string extension)
		{
			if (string.IsNullOrWhiteSpace(fullScriptName) || string.IsNullOrWhiteSpace(extension))
				return null;

			return fullScriptName.Replace(ScriptingPluginCorePlugin.DataScripts, string.Empty) + extension;
		}
	}


}