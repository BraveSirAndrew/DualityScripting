namespace ScriptingPlugin
{
	public interface IScriptCompilerService
	{
		ScriptCompilerResult TryCompile(string scriptName, string scriptPath, string script);
		void SetPdbEditor(IPdbEditor pdbEditor);
	}
}