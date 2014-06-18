using System.Collections.Generic;
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
		private Mock<IScriptProjectEditor> _projectEditorMock;
		private Mock<IScriptTemplate> _scriptTemplate;
		private const string SolutionPath = @"c:\dir\solutionFile.sln";
		private const string FsProjectPath = @"c:\dir\scripts\FSharp\FSharpScripts.fsproj";
		private const string CsProjectPath = @"c:\dir\scripts\CSharp\CSharpScripts.csproj";
		private const string OldPath = @"c:\dir\project\old";
		private const string NewPath = @"c:\dir\project\new\resource.res";

		[SetUp]
		public void Setup()
		{
			_fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{SolutionPath, new MockFileData("")}
			});
			_projectEditorMock = new Mock<IScriptProjectEditor>();
			_scriptTemplate = new Mock<IScriptTemplate>();

			_scriptResourceEvents = new ScriptResourceEvents(_fileSystem,
				new ProjectConstants()
					{
						FSharpScriptExtension = ".fs",
						FSharpProjectPath = FsProjectPath,
						CSharpScriptExtension = ".cs",
						CSharpProjectPath = CsProjectPath,
					},
				_projectEditorMock.Object);
			_scriptResourceEvents.AddDefaultScriptTemplate<CSharpScript>(_scriptTemplate.Object);
		}

		[Test]
		public void When_renaming_scripts_Then_old_script_removed_and_new_script_added()
		{
			_scriptTemplate.SetupGet(m => m.ProjectPath).Returns(CsProjectPath);
			_projectEditorMock.Setup(x => x.RemoveOldScriptFromProject(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
			_projectEditorMock.Setup(x => x.AddScriptToProject(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

			UpdateFileSystem(CsProjectPath, GetResourceContent(), _fileSystem);
			var resourceRenamedEventArgs = CreateEventArgs(NewPath, OldPath);

			_scriptResourceEvents.OnResourceRenamed(null, resourceRenamedEventArgs);

			_projectEditorMock.VerifyAll();
		}

		[Test]
		public void When_csharp_script_created_Then_apply_template()
		{
			var path = @"Data\Scripts\resource.res";
			
			_scriptResourceEvents.OnResourceCreated(null, new ResourceEventArgs(CreateContentRef(GetFSScriptText(), path)));

			_scriptTemplate.Verify(x => x.Apply(It.Is<ContentRef<ScriptResourceBase>>(s => s.Path == path)), Times.Once);
		}

		[Test]
		public void When_csharp_script_created_Then_add_script_to_project()
		{
			var path = @"Data\Scripts\resource.fsharp.res";
			
			_projectEditorMock.Setup(x => x.AddScriptToProject(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

			_scriptResourceEvents.OnResourceCreated(null, new ResourceEventArgs(CreateContentRef(GetFSScriptText(), path)));

			
			_projectEditorMock.VerifyAll();
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
			var cSharpScript = new CSharpScript()
			{
				Script = scriptText,
				SourcePath = path,
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