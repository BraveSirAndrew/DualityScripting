using System.CodeDom.Compiler;

namespace ScriptingPlugin
{
	public interface IScriptCompiler
	{
		CompilerResults Compile(string script);
	}
}