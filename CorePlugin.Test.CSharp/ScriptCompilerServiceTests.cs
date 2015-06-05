
using System.Collections;
using Moq;
using NUnit.Framework;
using ScriptingPlugin;

namespace CorePlugin.Test.CSharp
{
	[TestFixture]
    public class ScriptCompilerServiceTests
    {
		private Mock<IScriptCompilerResults> _compilerResult;
		private Mock<IScriptCompiler> _compiler;

		[SetUp]
		public void Setup()
		{
			_compiler = new Mock<IScriptCompiler>();
			_compilerResult = new Mock<IScriptCompilerResults>();
			_compilerResult.Setup(m => m.Errors).Returns(new[] { "{", "}" });
		}

		[Test]
		public void TryCompileOneDoesntThrowWhenErrorMessageContainsBraces()
		{
			_compiler.Setup(m => m.Compile(It.IsAny<string>(), It.IsAny<string>())).Returns(_compilerResult.Object);

			var service = new ScriptCompilerService(_compiler.Object);

			Assert.DoesNotThrow(() => service.TryCompile("", "", ""));
		}

		[Test]
		public void TryCompileManyDoesntThrowWhenErrorMessageContainsBraces()
		{
			_compiler.Setup(m => m.Compile(It.IsAny<CompilationUnit[]>(), It.IsAny<string>())).Returns(_compilerResult.Object);

			var service = new ScriptCompilerService(_compiler.Object);

			Assert.DoesNotThrow(() => service.TryCompile("", "", ""));
		}

		
    }
}
