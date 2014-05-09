using System;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Ionic.Zip;

namespace ScriptingPlugin.Editor
{
	public class ScriptsSolutionEditor
	{
		private readonly IFileSystem _fileSystem;
		private readonly string _sourceCodeDirectory;
		private const string SolutionProjectReferences = "\r\nProject(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"Scripts\", \"Scripts\\Scripts.csproj\", \"{1DC301F5-644D-4109-96C4-2158ABDED70D}\"\r\nEndProject";

		public ScriptsSolutionEditor(IFileSystem fileSystem, string sourceCodeDirectory)
		{
			_fileSystem = fileSystem;
			_sourceCodeDirectory = sourceCodeDirectory;
		}

		public void ExtractScriptProjectToCodeDirectory(string projectPath, byte[] projectTemplate)
		{
			if (_fileSystem.File.Exists(projectPath))
				return;

			if (!SolutionExists().Any()) 
				return;

			using (var scriptsProjectZip = ZipFile.Read(projectTemplate))
			{
				scriptsProjectZip.ExtractAll(Path.Combine(_sourceCodeDirectory, ScriptingEditorPlugin.Scripts),
					ExtractExistingFileAction.DoNotOverwrite);
			}
		}

		private string[] SolutionExists()
		{
			return _fileSystem.Directory.GetFiles(_sourceCodeDirectory, "*.sln");
		}

		public void AddScriptProjectToSolution()
		{
			var solutionExists = SolutionExists();
			if(solutionExists.Any() == false)
				return;

			var solutionPath = solutionExists.First();
			var slnText = _fileSystem.File.ReadAllText(solutionPath);

			if (slnText.ToString(CultureInfo.InvariantCulture).IndexOf(SolutionProjectReferences, StringComparison.OrdinalIgnoreCase) != -1)
				return;

			slnText = slnText.Insert(
				slnText.LastIndexOf("EndProject", StringComparison.OrdinalIgnoreCase) + 10, SolutionProjectReferences);
			_fileSystem.File.WriteAllText(solutionPath, slnText);
		}
	}
}