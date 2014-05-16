namespace ScriptingPlugin
{
	public interface IPdbEditor
	{
		ScriptsResult SetSourcePathInPdbFile(string pathToAssembly, string scriptName, string scriptPath);
	}
}