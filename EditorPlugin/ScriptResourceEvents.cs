using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Duality;
using Duality.Editor;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class ScriptResourceEvents
	{
		public const string MediaFolder = @"..\..\..\Media";

		private readonly IFileSystem _fileSystem;
		private readonly IScriptProjectEditor _projectEditor;
		private Dictionary<Type, IScriptTemplate> _scriptTemplates = new Dictionary<Type, IScriptTemplate>();

		public ScriptResourceEvents(IFileSystem fileSystem, ProjectConstants projectConstants, IScriptProjectEditor projectEditor = null)
		{
			_fileSystem = fileSystem;
			_projectEditor = projectEditor ?? new ScriptProjectEditor();
		}

		public void AddDefaultScriptTemplate<TContentType>(IScriptTemplate template) where TContentType:ScriptResourceBase
		{
			_scriptTemplates.Add(typeof(TContentType), template);
		}
		
		public void OnResourceCreated(object sender, ResourceEventArgs resourceEventArgs)
		{
			var template = GetScriptTemplate(resourceEventArgs);
			if (template == null)
				return;

			template.Apply(resourceEventArgs.Content.As<ScriptResourceBase>());

			var sourceFilePath = FileImportProvider.GenerateSourceFilePath(resourceEventArgs.Content, template.FileExtension);
			sourceFilePath = sourceFilePath.Replace("Source\\Media", MediaFolder);
			var scriptFileName = GetFileName(sourceFilePath);
			
			_projectEditor.AddScriptToProject(sourceFilePath, scriptFileName, template.ProjectPath);
		}

		public void OnResourceRenamed(object sender, ResourceRenamedEventArgs renamedEventArgs)
		{
			if (!renamedEventArgs.Content.Is(typeof(ScriptResourceBase)))
				return;

			var template = GetScriptTemplate(renamedEventArgs);
			if (template == null)
				return;

			if(template.ProjectPath == null)
				return;

			var projectPathComplete = Path.Combine(PathHelper.ExecutingAssemblyDir, template.ProjectPath);
			if (!_fileSystem.File.Exists(projectPathComplete))
				return;

			var oldName = renamedEventArgs.OldContent.FullName;
			_projectEditor.RemoveOldScriptFromProject(oldName, template.FileExtension, projectPathComplete);
			
			var sourceFilePath = renamedEventArgs.Content.Res.SourcePath;
			sourceFilePath = sourceFilePath.Replace("Source\\Media", MediaFolder);
			_projectEditor.AddScriptToProject(sourceFilePath, renamedEventArgs.Content.Res.Name + template.FileExtension, template.ProjectPath);
		}
		
		private string GetFileName(string fileWithPath)
		{
			return Path.GetFileName(fileWithPath);
		}

		private IScriptTemplate GetScriptTemplate(ResourceEventArgs resourceEventArgs)
		{
			IScriptTemplate template;
			if (_scriptTemplates.TryGetValue(resourceEventArgs.ContentType, out template) == false)
			{
				Log.Editor.WriteError("Unrecognized script type '{0}'", resourceEventArgs.ContentType);
				return template;
			}
			return template;
		}
	}
}