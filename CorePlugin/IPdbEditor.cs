namespace ScriptingPlugin
{
	public interface IPdbEditor
	{
		CompilerResult SetSourcePathInPdbFile(string pathToAssembly, string scriptName, string scriptPath);
	}
}