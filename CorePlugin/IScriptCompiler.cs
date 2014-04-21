using System.Reflection;

namespace ScriptingPlugin
{
	public interface IScriptCompiler
	{
		Assembly Compile(string scriptName, string scriptPath, string script);
	}
}