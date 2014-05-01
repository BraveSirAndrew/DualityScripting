using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Duality;
using Ionic.Zip;

namespace ScriptingPlugin.Editor
{
	public class ScriptsSolutionEditor
	{
		private readonly string _sourceCodeDirectory;
		private const string SolutionProjectReferences = "\nProject(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"Scripts\", \"Scripts\\Scripts.csproj\", \"{1DC301F5-644D-4109-96C4-2158ABDED70D}\"\nEndProject";

		public ScriptsSolutionEditor(string sourceCodeDirectory )
		{
			_sourceCodeDirectory = sourceCodeDirectory;
		}

		public void ModifySolution(string projectPath)
		{
			if (File.Exists(projectPath))
				return;
			try
			{
				ExtractScriptProjectToCodeDirectory();
				AddScriptProjectToSolution();
			}
			catch (Exception exception)
			{
				Log.Editor.WriteWarning("Tried adding a project to code directory and adding to solution but failed with the following exception {0} /n {1}", exception.Message, exception.StackTrace);
			}
			
		}

		private  void ExtractScriptProjectToCodeDirectory()
		{
			using (var scriptsProjectZip = ZipFile.Read(Resources.Resources.ScriptsProjectTemplate))
			{
				scriptsProjectZip.ExtractAll(Path.Combine(_sourceCodeDirectory, ScriptingEditorPlugin.Scripts),
					ExtractExistingFileAction.DoNotOverwrite);
			}
		}

		private  void AddScriptProjectToSolution()
		{
			var slnPath = Directory.GetFiles(_sourceCodeDirectory, "*.sln").First();
			var slnText = File.ReadAllText(slnPath);

			if (!slnText.Any(x => x.ToString(CultureInfo.InvariantCulture).Contains(SolutionProjectReferences)))
			{
				slnText = slnText.Insert(slnText.LastIndexOf("EndProject", StringComparison.OrdinalIgnoreCase) + 10,
					SolutionProjectReferences);
			}
			File.WriteAllText(slnPath, slnText);
		}
	}
}