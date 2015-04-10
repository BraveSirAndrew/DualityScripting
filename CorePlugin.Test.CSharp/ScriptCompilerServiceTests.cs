
using Moq;
using NUnit.Framework;
using ScriptingPlugin;

namespace CorePlugin.Test.CSharp
{
	[TestFixture]
    public class ScriptCompilerServiceTests
    {
		[Test]
		public void TryCompileDoesntThrowWhenErrorMessageContainsBraces()
		{
			var compiler = new Mock<IScriptCompiler>();
			var compilerResult = new Mock<IScriptCompilerResults>();
		    
			compilerResult.Setup(m => m.Errors).Returns(new[] {"{", "}"});
			compiler.Setup(m => m.Compile(It.IsAny<string>(), It.IsAny<string>())).Returns(compilerResult.Object);

			var service = new ScriptCompilerService(compiler.Object);

			Assert.DoesNotThrow(() => service.TryCompile("", "", ""));
		}
    }
}
