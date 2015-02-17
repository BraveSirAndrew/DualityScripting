using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using Duality;
using Duality.Editor;
using Duality.Resources;
using Moq;
using NUnit.Framework;
using ScriptingPlugin.CSharp;
using ScriptingPlugin.Editor;
using ScriptingPlugin.Resources;

namespace EditorPlugin.Tests
{
	[TestFixture]
	public class ScriptResourceEventsTests
	{
		private ScriptResourceEvents _scriptResourceEvents;
		private MockFileSystem _fileSystem;
		private Mock<IScriptProjectEditor> _projectEditorMock;
		private Mock<IScriptTemplate> _scriptTemplate;
		private Mock<ISourceFilePathGenerator> _sourceFilePathGenerator;
		private const string SolutionPath = @"c:\dir\solutionFile.sln";
		private const string FsProjectPath = @"c:\dir\scripts\FSharp\FSharpScripts.fsproj";
		private const string CsProjectPath = @"c:\dir\scripts\CSharp\CSharpScripts.csproj";
		private const string OldPath = @"Data\Path\oldResource.CSharpScript.res";
		private const string NewPath = @"Data\Path\resource.CSharpScript.res";

		[SetUp]
		public void Setup()
		{
			_fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{SolutionPath, new MockFileData("")}
			});
			_projectEditorMock = new Mock<IScriptProjectEditor>();
			_scriptTemplate = new Mock<IScriptTemplate>();

			_sourceFilePathGenerator = new Mock<ISourceFilePathGenerator>();
			_scriptResourceEvents = new ScriptResourceEvents(_fileSystem, 
				_sourceFilePathGenerator.Object,
				_projectEditorMock.Object);
			_scriptResourceEvents.AddDefaultScriptTemplate<CSharpScript>(_scriptTemplate.Object);
		}

		[Test]
		public void When_non_resource_is_renamed_Then_dont_throw()
		{
			var eventArgs = new ResourceRenamedEventArgs("", "");

			Assert.DoesNotThrow(() => _scriptResourceEvents.OnResourceRenamed(this, eventArgs));
		}

		[Test]
		public void When_renaming_scripts_Then_old_script_removed_and_new_script_added()
		{
			_scriptTemplate.SetupGet(m => m.ProjectPath).Returns(CsProjectPath);
			_projectEditorMock.Setup(x => x.RemoveScriptFromProject(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
			_projectEditorMock.Setup(x => x.AddScriptToProject(Path.Combine(ProjectConstants.MediaFolder, "Path\\resource.cs"), "resource.cs", It.IsAny<string>()));

			UpdateFileSystem(CsProjectPath, GetResourceContent(), _fileSystem);
			var resourceRenamedEventArgs = CreateEventArgs(NewPath, OldPath);

			_scriptResourceEvents.OnResourceRenamed(null, resourceRenamedEventArgs);

			_projectEditorMock.VerifyAll();
		}

		[Test]
		public void When_renaming_a_script_that_has_never_been_opened_Then_ask_for_a_new_source_path()
		{
			var sourceFilePath = @"Source\Media\Test.cs";

			_scriptTemplate.SetupGet(m => m.ProjectPath).Returns(CsProjectPath);
			_sourceFilePathGenerator.Setup(m => m.GenerateSourceFilePath(It.IsAny<ContentRef<Resource>>(), It.IsAny<string>())).Returns(sourceFilePath);
			_projectEditorMock.Setup(m => m.AddScriptToProject(Path.Combine(ProjectConstants.MediaFolder, "Test.cs"), It.IsAny<string>(), It.IsAny<string>()));

			var resourceRenamedEventArgs = CreateEventArgs(NewPath, OldPath);
			resourceRenamedEventArgs.Content.Res.SourcePath = null;


			UpdateFileSystem(CsProjectPath, GetResourceContent(), _fileSystem);
			_scriptResourceEvents.OnResourceRenamed(null, resourceRenamedEventArgs);

			_projectEditorMock.VerifyAll();
		}

		[Test]
		public void When_a_non_resources_is_created_Then_dont_throw()
		{
			var eventArgs = new ResourceEventArgs(new ContentRef<Resource>());

			Assert.DoesNotThrow(() => _scriptResourceEvents.OnResourceCreated(this, eventArgs));
		}

		[Test]
		public void When_csharp_script_created_Then_apply_template()
		{
			var path = @"Data\Scripts\resource.res";

			_sourceFilePathGenerator.Setup(m => m.GenerateSourceFilePath(It.IsAny<ContentRef<Resource>>(), It.IsAny<string>())).Returns(@"Source\Media\Scripts\resource.cs");
			_scriptResourceEvents.OnResourceCreated(null, new ResourceEventArgs(CreateContentRef(GetFSScriptText(), path)));

			_scriptTemplate.Verify(x => x.Apply(It.Is<ContentRef<ScriptResourceBase>>(s => s.Path == path)), Times.Once);
		}

		[Test]
		public void When_csharp_script_created_Then_add_script_to_project()
		{
			_sourceFilePathGenerator.Setup(m => m.GenerateSourceFilePath(It.IsAny<ContentRef<Resource>>(), It.IsAny<string>())).Returns(@"Source\Media\Scripts\resource.cs");
			_projectEditorMock.Setup(x => x.AddScriptToProject(Path.Combine(ProjectConstants.MediaFolder, @"Scripts\resource.cs"), "resource.cs", It.IsAny<string>()));

			_scriptResourceEvents.OnResourceCreated(null, new ResourceEventArgs(CreateContentRef(GetFSScriptText(), null)));

			
			_projectEditorMock.VerifyAll();
		}

		[Test]
		public void When_non_resource_is_deleted_Then_dont_throw()
		{
			var eventArgs = new ResourceEventArgs(new ContentRef<Resource>());

			Assert.DoesNotThrow(() => _scriptResourceEvents.OnResourceDeleting(this, eventArgs));
		}

		[Test]
		public void When_script_deleted_Then_link_is_removed_from_project()
		{
			_projectEditorMock.Setup(m => m.RemoveScriptFromProject(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

			_scriptResourceEvents.OnResourceDeleting(null, new ResourceEventArgs(CreateContentRef(GetCSScriptText(), "")));

			_projectEditorMock.VerifyAll();
		}

		[Test]
		public void When_script_with_no_sourcePath_is_deleted_Then_use_generated_source_path_name()
		{
			var resourceEventArgs = new ResourceEventArgs(CreateContentRef(GetCSScriptText(), "Path\\test"));
			resourceEventArgs.Content.Res.SourcePath = null;
			
			_sourceFilePathGenerator.Setup(m => m.GenerateSourceFilePath(It.IsAny<ContentRef<Resource>>(), It.IsAny<string>())).Returns(@"Path\test (2).cs");
			_projectEditorMock.Setup(m => m.RemoveScriptFromProject(@"Path\test (2)", It.IsAny<string>(), It.IsAny<string>()));

			_scriptResourceEvents.OnResourceDeleting(null, resourceEventArgs);

			_projectEditorMock.VerifyAll();
		}

		[Test]
		public void When_a_non_script_resource_is_renamed_Then_dont_throw()
		{
			var resource = new Prefab();
			resource.Save("test.prefab.res");
			var eventArgs = new ResourceRenamedEventArgs("test2.prefab.res", "test.prefab.res");

			Assert.DoesNotThrow(() => _scriptResourceEvents.OnResourceRenamed(this, eventArgs));
		}

		[Test]
		public void When_a_non_script_resource_is_created_Then_dont_run()
		{
			var logCountBeforeRunning = Log.LogData.Data.Count();
			
			var resource = new Prefab();
			resource.Save("test.prefab.res");
			var eventArgs = new ResourceEventArgs("test.prefab.res");

			_scriptResourceEvents.OnResourceCreated(this, eventArgs);

			Assert.AreEqual(logCountBeforeRunning, Log.LogData.Data.Count());
		}

		[Test]
		public void When_a_non_script_resource_is_deleted_Then_dont_run()
		{
			var logCountBeforeRunning = Log.LogData.Data.Count();

			var resource = new Prefab();
			resource.Save("test.prefab.res");
			var eventArgs = new ResourceEventArgs("test.prefab.res");

			_scriptResourceEvents.OnResourceDeleting(this, eventArgs);

			Assert.AreEqual(logCountBeforeRunning, Log.LogData.Data.Count());
		}

		private ResourceRenamedEventArgs CreateEventArgs(string newPath, string oldPath)
		{
			var eventArgs = new ResourceRenamedEventArgs(newPath, oldPath);

			var fieldInfo = eventArgs.GetType().BaseType.GetField("content", BindingFlags.NonPublic | BindingFlags.Instance);
			if (fieldInfo != null)
				fieldInfo.SetValue(eventArgs, CreateContentRef(GetCSScriptText(),newPath));

			return eventArgs;
		}

		private string GetCSScriptText()
		{
			return @"namespace Tests
                        {
                            public class MyClass { 
                                public void MyMethod(){var c= 1;}
                            }
                        }";
		}

		private static ContentRef<Resource> CreateContentRef(string scriptText, string path)
		{
			var sourcePath = path;
			
			if(!string.IsNullOrEmpty(sourcePath))
			{
				sourcePath = path
					.Replace(DualityApp.DataDirectory, "")
					.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				sourcePath = sourcePath.Remove(sourcePath.IndexOf('.') + 1) + "cs";
			}
			
			var cSharpScript = new CSharpScript
			{
				Script = scriptText,
				SourcePath = Path.Combine(@"Source\Media", sourcePath ?? ""),
			};
			var fieldInfo = cSharpScript.GetType().GetField("path", BindingFlags.NonPublic | BindingFlags.Instance);
			if (fieldInfo != null)
				fieldInfo.SetValue(cSharpScript, path);

			return new ContentRef<Resource>(cSharpScript);
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

		private static string GetResourceContent()
		{
			return @"<root dataType=""Struct"" type=""ScriptingPlugin.Resources.CSharpScript"" id=""129723834"">
  <_x003C_Script_x003E_k__BackingField dataType=""String"">using Duality;
using OpenTK;
using ScriptingPlugin;

namespace ScriptingNamespace
{
	public class MoveBall : DualityScript
	{
		public override void Update()
		{
			GameObj.Transform.Pos += new Vector3(1.0f, .0f, .0f);
		}
	}
}</_x003C_Script_x003E_k__BackingField>
  <sourcePath dataType=""String"">Source\Media\Scripts\MoveBall.cs</sourcePath>
</root>
<!-- XmlFormatterBase Document Separator -->
";
		}

		private static void UpdateFileSystem(string path, string fileContent, MockFileSystem fileSystem)
		{
			if(!fileSystem.FileExists(path))
				fileSystem.AddFile(path, new MockFileData(fileContent));
			Assert.True(fileSystem.FileExists(path), "File {0} doesn't exist but it should.", path);
		}

	}
}