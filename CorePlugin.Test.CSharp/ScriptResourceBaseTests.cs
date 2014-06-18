using Moq;
using NUnit.Framework;
using ScriptingPlugin;
using ScriptingPlugin.Resources;

namespace CorePlugin.Test.CSharp
{
	public class ScriptResourceBaseTests
	{
		[TestFixture]
		public class TheReloadMethod
		{
			private TestScriptType _script;
			private Mock<IScriptCompilerService> _compilerService;
			private Mock<IScriptMetadataService> _metadataService;

			[SetUp]
			public void SetUp()
			{
				_compilerService = new Mock<IScriptCompilerService>();
				_metadataService = new Mock<IScriptMetadataService>();
				_script = new TestScriptType
				{
					ScriptCompilerServiceProxy = _compilerService.Object,
					ScriptMetadataServiceProxy = _metadataService.Object,
					SourcePath = "test"
				};
			}

			[Test]
			public void RecompilesTheScript()
			{
				_script.Reload();

				_compilerService.Verify(m => m.TryCompile(It.IsAny<string>(), "test", It.IsAny<string>()));
			}

			[Test]
			public void UpdatesTheMetadata()
			{
				_script.Save("testpath");
				_script.Reload();

				_metadataService.Verify(m => m.UpdateMetadata("testpath"), Times.Once);
			}
		}

		public class TheAssemblyProperty
		{
			[Test]
			public void ReturnsANewAssemblyAfterReloading()
			{
				var script = new TestScriptType
				{
					ScriptCompilerServiceProxy = new ScriptCompilerService(new CSharpScriptCompiler(), null),
					ScriptMetadataServiceProxy = new Mock<IScriptMetadataService>().Object,
					SourcePath = "test",
					Script = "namespace Tests { public class MyClass { }}"
				};

				var originalAssembly = script.Assembly;

				script.Reload();

				Assert.AreNotEqual(originalAssembly, script.Assembly);
			}
		}

		private class TestScriptType : ScriptResourceBase
		{
			public IScriptCompilerService ScriptCompilerServiceProxy
			{
				get { return ScriptCompiler; }
				set { ScriptCompiler = value; }
			}

			public IScriptMetadataService ScriptMetadataServiceProxy
			{
				get { return ScriptMetadataService; }
				set { ScriptMetadataService = value; }
			}
		}
	}
}
