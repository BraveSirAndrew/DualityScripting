namespace ScriptingPlugin
{
	public interface IScriptCompiler
	{
		IScriptCompilerResults Compile(string script, string sourceFilePath = null);
	}
}