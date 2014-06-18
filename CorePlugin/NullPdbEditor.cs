namespace ScriptingPlugin
{
	public class NullPdbEditor : IPdbEditor
	{
		public CompilerResult SetSourcePathInPdbFile(string pathToAssembly, string scriptName, string scriptPath)
		{
			return new CompilerResult();
		}
	}
}