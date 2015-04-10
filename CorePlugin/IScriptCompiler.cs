using System.Collections.Generic;

namespace ScriptingPlugin
{
	public interface IScriptCompiler
	{
		IScriptCompilerResults Compile(string script, string sourceFilePath);
		IScriptCompilerResults Compile(IEnumerable<CompilationUnit> scripts, string resultingAssemblyDirectory = null);
		void AddReference(string referenceAssembly);
	}
}