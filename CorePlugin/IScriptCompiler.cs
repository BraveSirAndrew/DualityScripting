using System.CodeDom.Compiler;

namespace ScriptingPlugin
{
	public interface IScriptCompiler
	{
		ScriptCompilerResults Compile(string script);
	}
}