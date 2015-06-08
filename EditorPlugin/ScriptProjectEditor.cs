using System;
using System.IO;
using System.Linq;
using Duality;
using Microsoft.Build.Construction;

namespace ScriptingPlugin.Editor
{
	public class ScriptProjectEditor : IScriptProjectEditor
	{
		public void AddScriptToProject(string scriptPath, string scriptFileName, string projectPath)
		{
			var directoryPart = scriptPath
					.Replace(ProjectConstants.MediaFolder, "")
					.Replace(Path.GetFileName(scriptPath), "")
					.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			AddScriptToProject(scriptPath, directoryPart, scriptFileName, projectPath);
		}

		public void AddScriptToProject(string scriptPath, string directoryPart, string scriptFileName, string projectPath)
		{
			try
			{
				CreateAnyMissingDirectories(projectPath, directoryPart);

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

		public void RemoveScriptFromProject(string oldContentName, string extension, string projectPath)
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
						itemGroup.RemoveChild(item);
					}
				}
			}
			rootElement.Save();
		}

		public void RemoveAllScriptReferences(string projectPath)
		{
			var rootElement = ProjectRootElement.Open(projectPath);
			if (rootElement == null)
				return;

			var scriptGroup = rootElement.ItemGroups.FirstOrDefault(g => g.Items.Any(i => i.ItemType == "Compile"));
			if (scriptGroup != null)
			{
				scriptGroup.RemoveAllChildren();
				rootElement.RemoveChild(scriptGroup);
			}

			rootElement.Save();
		}

		private string GetScriptNameWithPath(string fileNameWithResourcePath)
		{
			return Path.Combine(@"..\..\..\Media", fileNameWithResourcePath);
		}

		private string RemoveDataScriptPath(string fullScriptName, string extension)
		{
			if (string.IsNullOrWhiteSpace(fullScriptName) || string.IsNullOrWhiteSpace(extension))
				return null;

			return fullScriptName.Replace(DualityApp.DataDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + extension;
		}

		private static void CreateAnyMissingDirectories(string projectPath, string directoryPart)
		{
			var directories = directoryPart.Split(new[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar},
				StringSplitOptions.RemoveEmptyEntries);

			var currentDirectory = Path.GetDirectoryName(projectPath);
			foreach (var directory in directories)
			{
				var combine = Path.Combine(currentDirectory, directory);
				if (Directory.Exists(combine) == false)
					Directory.CreateDirectory(combine);

				currentDirectory = Path.Combine(currentDirectory, directory);
			}
		}
	}
}