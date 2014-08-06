using System;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Duality.Editor;
using Ionic.Zip;

namespace ScriptingPlugin.Editor
{
	public class ScriptsSolutionEditor
	{
		private readonly IFileSystem _fileSystem;
		private readonly string _sourceCodeDirectory;
		private const string SolutionCSharpProjectReferences = "\r\nProject(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"Scripts\", \"Scripts\\CSharp\\Scripts.csproj\", \"{1DC301F5-644D-4109-96C4-2158ABDED70D}\"\r\nEndProject";
		private const string SolutionFSharpProjectReferences = "\r\nProject(\"{F2A71F9B-5D33-465A-A702-920D77279786}\") = \"FSharpScripts\", \"Scripts\\FSharp\\FSharpScripts.fsproj\", \"{42BBD2A5-7D56-4935-A8E4-9FA236F472CF}\"\r\nEndProject";

		public ScriptsSolutionEditor(IFileSystem fileSystem, string sourceCodeDirectory)
		{
			_fileSystem = fileSystem;
			_sourceCodeDirectory = sourceCodeDirectory;
		}

		public string AddToSolution(string projectLanguagePath, string projectName, byte[] projectTemplate)
		{
			const string scripts = "Scripts";
			var projectPath = Path.Combine(EditorHelper.SourceCodeDirectory, scripts, projectLanguagePath,  projectName);
			ExtractScriptProjectToCodeDirectory(projectPath, projectTemplate);
			AddScriptsProjectsToSolution();
			return projectPath;
		}
		
		public void ExtractScriptProjectToCodeDirectory(string projectPath, byte[] projectTemplate)
		{
			if (_fileSystem.File.Exists(projectPath))
				return;

			if (SolutionExists() == false)
				return;

			using (var scriptsProjectZip = ZipFile.Read(projectTemplate))
			{
				scriptsProjectZip.ExtractAll(Path.GetDirectoryName(projectPath), ExtractExistingFileAction.DoNotOverwrite);
			}
		}

		private bool SolutionExists()
		{
			return _fileSystem.File.Exists(GetSolutionPath());
		}

		private string GetSolutionPath()
		{
			return _fileSystem.Directory.GetFiles(_sourceCodeDirectory, "*.sln").FirstOrDefault();
		}

		public void AddScriptsProjectsToSolution()
		{
			
			if (SolutionExists() == false)
				return;

			var solutionPath = GetSolutionPath();
			var slnText = _fileSystem.File.ReadAllText(solutionPath);

			AddProjectToSolution(slnText, solutionPath, SolutionCSharpProjectReferences);
		}

		private void AddProjectToSolution(string slnText, string solutionPath, string projectReference)
		{
			if (slnText.ToString(CultureInfo.InvariantCulture)
					.IndexOf(projectReference, StringComparison.OrdinalIgnoreCase) != -1)
				return;

			slnText = slnText.Insert(
				slnText.LastIndexOf("EndProject", StringComparison.OrdinalIgnoreCase) + 10, projectReference);
			_fileSystem.File.WriteAllText(solutionPath, slnText);
		}
	}
}