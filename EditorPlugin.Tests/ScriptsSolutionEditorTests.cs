using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using ScriptingPlugin.Editor;

namespace EditorPlugin.Tests
{
	[TestFixture]
	public class ScriptsSolutionEditorTests
	{
		public class TheAddScriptProjectToSolutionMethod
		{
			private const string _projectReferenceString = "\r\nProject(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"Scripts\", \"Scripts\\Scripts.csproj\", \"{1DC301F5-644D-4109-96C4-2158ABDED70D}\"\r\nEndProject";
			private const string _solutionPath = @"c:\dir\solutionFile.sln";
			private const string _sourceCodeDirectory = "c:\\dir";

			[Test]
			public void AddsAReferenceToTheScriptsProjectToTheSolutionFile()
			{
				var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
				{
					{_solutionPath, new MockFileData("EndProject")}
				});
				

				var solutionEditor = new ScriptsSolutionEditor(fileSystem, _sourceCodeDirectory);
				solutionEditor.AddScriptProjectToSolution();


				Assert.AreEqual("EndProject" + _projectReferenceString, new MockFile(fileSystem).ReadAllText(_solutionPath));
			}

			[Test]
			public void DoesntAddAReferenceToTheScriptsProjectIfItAlreadyExists()
			{
				var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
				{
					{_solutionPath, new MockFileData(_projectReferenceString)}
				});


				var solutionEditor = new ScriptsSolutionEditor(fileSystem, _sourceCodeDirectory);
				solutionEditor.AddScriptProjectToSolution();


				Assert.AreEqual(_projectReferenceString, new MockFile(fileSystem).ReadAllText(_solutionPath));
			}

			[Test]
			public void IsCaseInsensitive()
			{
				var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
				{
					{_solutionPath, new MockFileData(_projectReferenceString.ToLower())}
				});


				var solutionEditor = new ScriptsSolutionEditor(fileSystem, _sourceCodeDirectory);
				solutionEditor.AddScriptProjectToSolution();


				Assert.IsTrue(_projectReferenceString.Equals(new MockFile(fileSystem).ReadAllText(_solutionPath), StringComparison.OrdinalIgnoreCase));
			}

			[Test]
			public void When_Solution_doesnt_exist_then_it_doesnt_throw()
			{
				var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
				{
					{Path.Combine(_sourceCodeDirectory, "file.txt"), new MockFileData("file.txt")}
				});

				var solutionEditor = new ScriptsSolutionEditor(fileSystem, _sourceCodeDirectory);
				Assert.DoesNotThrow(solutionEditor.AddScriptProjectToSolution);
				
			}
		}
	}
}
