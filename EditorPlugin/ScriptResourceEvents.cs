using System.IO;
using System.IO.Abstractions;
using Duality;
using Duality.Editor;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class ScriptResourceEvents
	{
		private readonly IFileSystem _fileSystem;
		private readonly ProjectConstants _projectConstants;
		private readonly IScriptProjectEditor _projectEditor;
		private readonly IResourceSaver _resourceSaver;

		public ScriptResourceEvents(IFileSystem fileSystem, ProjectConstants projectConstants, IResourceSaver resourceSaver = null , IScriptProjectEditor projectEditor = null)
		{
			_fileSystem = fileSystem;
			_projectConstants = projectConstants;
			_projectEditor = projectEditor ?? new ScriptProjectEditor();
			_resourceSaver = resourceSaver ?? new ResourceSaver(_projectConstants);
		}
		
		public void OnResourceCreated(object sender, ResourceEventArgs resourceEventArgs)
		{
			var resourceData = _resourceSaver.Save(resourceEventArgs);
			var fileWithPath = RemoveDataScriptPath(resourceData.ScriptFullName, resourceData.ScriptExtension);
			if(!string.IsNullOrWhiteSpace(fileWithPath))
				_projectEditor.AddScriptToProject(GetScriptNameWithPath(fileWithPath), GetFileName(fileWithPath), resourceData.ProjectPath);
		}

		
		public void OnResourceRenamed(object sender, ResourceRenamedEventArgs e)
		{
			string extension = null;
			string projectPath = null;
			if (!(typeof(ScriptResourceBase)).IsAssignableFrom(e.ContentType))
				return;
			if (e.ContentType == typeof(CSharpScript))
			{
				extension = _projectConstants.CSharpScriptExtension;
				projectPath = _projectConstants.CSharpProjectPath;
			}
			else if (e.ContentType == typeof(FSharpScript))
			{
				extension = _projectConstants.FSharpScriptExtension;
				projectPath = _projectConstants.FSharpProjectPath;
			}
			
			if(projectPath == null)
				return;
			var projectPathComplete = Path.Combine(PathHelper.ExecutingAssemblyDir, projectPath);
			if (!_fileSystem.File.Exists(projectPathComplete))
				return;

			var oldName = e.OldContent.FullName;
			_projectEditor.RemoveOldScriptFromProject(oldName, extension, projectPathComplete);
			string fileNameWithResourcePath = RemoveDataScriptPath(e.Content.FullName, extension);
			if(string.IsNullOrWhiteSpace(fileNameWithResourcePath))
				return;
			var newScriptName = GetScriptNameWithPath(fileNameWithResourcePath);
			_projectEditor.AddScriptToProject(newScriptName, GetFileName(newScriptName), projectPath);
		}


		private string GetFileName(string fileWithPath)
		{
			return Path.GetFileName(fileWithPath);
		}

		private string GetScriptNameWithPath(string fileNameWithResourcePath)
		{
			const string Scripts = "Scripts";
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