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
		private readonly ISourceFilePathGenerator _sourceFilePathGenerator;
		private readonly IScriptProjectEditor _projectEditor;
		private Dictionary<Type, IScriptTemplate> _scriptTemplates = new Dictionary<Type, IScriptTemplate>();

		public ScriptResourceEvents(IFileSystem fileSystem, ISourceFilePathGenerator sourceFilePathGenerator, IScriptProjectEditor projectEditor = null)
		{
			_fileSystem = fileSystem;
			_sourceFilePathGenerator = sourceFilePathGenerator;
			_projectEditor = projectEditor ?? new ScriptProjectEditor();
		}

		public void AddDefaultScriptTemplate<TContentType>(IScriptTemplate template) where TContentType:ScriptResourceBase
		{
			_scriptTemplates.Add(typeof(TContentType), template);
		}
		
		public void OnResourceCreated(object sender, ResourceEventArgs resourceEventArgs)
		{
			if (resourceEventArgs.IsResource == false)
				return;

			if (IsResourceAScript(resourceEventArgs) == false)
				return;

			var template = GetScriptTemplate(resourceEventArgs);
			if (template == null)
				return;

			template.Apply(resourceEventArgs.Content.As<ScriptResourceBase>());

			var sourceFilePath = _sourceFilePathGenerator.GenerateSourceFilePath(resourceEventArgs.Content, template.FileExtension);
			sourceFilePath = sourceFilePath.Replace("Source\\Media", MediaFolder);
			var scriptFileName = GetFileName(sourceFilePath);
			
			_projectEditor.AddScriptToProject(sourceFilePath, scriptFileName, template.ProjectPath);
		}

		public void OnResourceRenamed(object sender, ResourceRenamedEventArgs renamedEventArgs)
		{
			if (renamedEventArgs.IsResource == false)
				return;

			if (IsResourceAScript(renamedEventArgs) == false)
				return;

			var template = GetScriptTemplate(renamedEventArgs);
			if (template == null)
				return;

			if(template.ProjectPath == null)
				return;

			var projectPathComplete = Path.Combine(Environment.CurrentDirectory, template.ProjectPath);
			if (!_fileSystem.File.Exists(projectPathComplete))
				return;

			var oldName = renamedEventArgs.OldContent.FullName;
			_projectEditor.RemoveScriptFromProject(oldName, template.FileExtension, projectPathComplete);
			
			var sourceFilePath = renamedEventArgs.Content.Res.SourcePath;

			// SourcePath can be null in the case where the user created a new script and renamed it straight away, or when the script has never been opened from Duality
			if (string.IsNullOrEmpty(sourceFilePath))
				sourceFilePath = _sourceFilePathGenerator.GenerateSourceFilePath(renamedEventArgs.Content, template.FileExtension);

			sourceFilePath = sourceFilePath.Replace("Source\\Media", MediaFolder);
			_projectEditor.AddScriptToProject(sourceFilePath, Path.GetFileName(sourceFilePath), template.ProjectPath);
		}

		public void OnResourceDeleting(object sender, ResourceEventArgs resourceEventArgs)
		{
			if (resourceEventArgs.IsResource == false)
				return;

			if (IsResourceAScript(resourceEventArgs) == false)
				return;

			var template = GetScriptTemplate(resourceEventArgs);
			if (template == null)
				return;

			var scriptName = resourceEventArgs.Content.FullName;

			if (resourceEventArgs.Content.Res == null || resourceEventArgs.Content.Res.SourcePath == null)
			{
				var sourceFileName = Path.GetFileNameWithoutExtension(_sourceFilePathGenerator.GenerateSourceFilePath(resourceEventArgs.Content, template.FileExtension));
				scriptName = scriptName.Remove(scriptName.LastIndexOf('\\') + 1) + sourceFileName;
			}
			
			_projectEditor.RemoveScriptFromProject(scriptName, template.FileExtension, template.ProjectPath);
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

		private static bool IsResourceAScript(ResourceEventArgs resourceEventArgs)
		{
			return typeof(ScriptResourceBase).IsAssignableFrom(resourceEventArgs.ContentType);
		}
	}
}