using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Duality;
using Duality.Editor;
using Moq;
using NUnit.Framework;
using ScriptingPlugin.Editor;
using ScriptingPlugin.Resources;

namespace EditorPlugin.Tests
{
	[TestFixture]
	public class ScriptResourceEventsTests
	{
		private ScriptResourceEvents _scriptResourceEvents;
		private MockFileSystem _fileSystem;
		private Mock<IResourceSaver> _resourceSaverMock;
		private Mock<IScriptProjectEditor> _projectEditorMock;
		private const string SolutionPath = @"c:\dir\solutionFile.sln";
		private const string FsProjectPath = @"c:\dir\scripts\FSharp\FSharpScripts.fsproj";
		private const string OldPath = @"c:\dir\project\old";
		private const string NewPath = @"c:\dir\project\new";

		[SetUp]
		public void Setup()
		{
			_fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{SolutionPath, new MockFileData("")}
			});
			_resourceSaverMock = new Mock<IResourceSaver>();
			_projectEditorMock = new Mock<IScriptProjectEditor>();
			
			_scriptResourceEvents = new ScriptResourceEvents(_fileSystem, new ProjectConstants()
							{	FSharpScriptExtension = ".fs",
								FSharpProjectPath = FsProjectPath}, 
							_resourceSaverMock.Object,
							_projectEditorMock.Object);
		}

		[Test]
		public void When_renaming_scripts_Then_check_project_exists()
		{
			_scriptResourceEvents.OnResourceRenamed(null, CreateEventArgs());
		}


		[Test]
		public void When_cs_script_Then_add_to_csproj()
		{
			var path = @"c:\dir\project\Data\Scripts\resource.res";
			_resourceSaverMock.Setup(x => x.Save(It.IsAny<ResourceEventArgs>()))
				.Returns(new ResourceSaver.ResourceData(path, ".fs", FsProjectPath));
			_projectEditorMock.Setup(x => x.AddScriptToProject(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

			_scriptResourceEvents.OnResourceCreated(null, new ResourceEventArgs(CreateContentRef(GetFSScriptText(), path)));
			
			_resourceSaverMock.Verify(x=> x.Save(It.IsAny<ResourceEventArgs>()),Times.Once);
			_projectEditorMock.VerifyAll();
		}

		private static string GetFSScriptText()
		{
			var scriptText = @"module Dualityscript

open ScriptingPlugin
open Duality
open System

    type FSharpScript() =
        inherit DualityScript()";
			return scriptText;
		}

		private static ContentRef<CSharpScript> CreateContentRef(string scriptText, string path)
		{
			var cSharpScript = new CSharpScript()
			{
				Script = scriptText,
				SourcePath = path, 
			};
			var fieldInfo = cSharpScript.GetType().GetField("path", BindingFlags.NonPublic | BindingFlags.Instance);
			if (fieldInfo != null) 
				fieldInfo.SetValue(cSharpScript, path);

			return new ContentRef<CSharpScript>(cSharpScript);
		}

		private void UpdateFileSystem(string path, string fileContent)
		{
			_fileSystem.AddFile(path, new MockFileData(fileContent));
		}

		private ResourceRenamedEventArgs CreateEventArgs()
		{
			var content = new ContentRef<FSharpScript>(new FSharpScript() {Script = "asdasd"});
			var eventArgs = new ResourceRenamedEventArgs(content);
			
			return eventArgs;
		}
	}
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
