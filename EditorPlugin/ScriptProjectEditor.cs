using System;
using System.IO;
using System.Linq;
using Duality;
using Microsoft.Build.Construction;

namespace ScriptingPlugin.Editor
{
	public class ScriptProjectEditor : IScriptProjectEditor
	{
		private const string Scripts = "Scripts";

		public void AddScriptToProject(string scriptPath, string scriptFileName, string projectPath)
		{
			try
			{
				var directoryPart = scriptPath
					.Replace(ScriptResourceEvents.MediaFolder, "")
					.Replace(Path.GetFileName(scriptPath), "")
					.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

				var directories = directoryPart.Split(new []{Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);

				var currentDirectory = Path.GetDirectoryName(projectPath);
				foreach (var directory in directories)
				{
					var combine = Path.Combine(currentDirectory, directory);
					if (Directory.Exists(combine) == false)
						Directory.CreateDirectory(combine);

					currentDirectory = Path.Combine(currentDirectory, directory);
				}

				var rootElement = ProjectRootElement.Open(Path.Combine(PathHelper.ExecutingAssemblyDir, projectPath));
				if (rootElement == null)
					return;
				ProjectItemGroupElement itemGroup = null;
				foreach (var projectItemGroupElement in rootElement.ItemGroups)
				{
					if (projectItemGroupElement.Items.Any(x => x.ItemType == "Compile"))
						 itemGroup = projectItemGroupElement;
				}
				if(itemGroup == null)
					itemGroup = rootElement.AddItemGroup();

				var itemElement = itemGroup.AddItem("Compile", scriptPath);
				itemElement.AddMetadata("Link", Path.Combine(directoryPart, scriptFileName));
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
			return Path.Combine(@"..\..\..\Media", Scripts, fileNameWithResourcePath);
		}

		private string RemoveDataScriptPath(string fullScriptName, string extension)
		{
			if (string.IsNullOrWhiteSpace(fullScriptName) || string.IsNullOrWhiteSpace(extension))
				return null;

			return fullScriptName.Replace(ScriptingPluginCorePlugin.DataScripts, string.Empty) + extension;
		}
	}


}