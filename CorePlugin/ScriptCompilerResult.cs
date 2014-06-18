using System.Reflection;

namespace ScriptingPlugin
{
	public class ScriptCompilerResult
	{
		private readonly Assembly _compiledAssembly;

		public ScriptCompilerResult(CompilerResult compilerResult, Assembly compiledAssembly = null)
		{
			_compiledAssembly = compiledAssembly;
			CompilerResult = compilerResult;
		}

		public CompilerResult CompilerResult { get; set; }

		public Assembly Assembly
		{
			get { return _compiledAssembly; }
		}
	}
}