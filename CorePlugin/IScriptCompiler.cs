using System.CodeDom.Compiler;

namespace ScriptingPlugin
{
	public interface IScriptCompiler
	{
		IScriptCompilerResults Compile(string script);
	}
}