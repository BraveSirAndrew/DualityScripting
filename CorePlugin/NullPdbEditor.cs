namespace ScriptingPlugin
{
	public class NullPdbEditor : IPdbEditor
	{
		public ScriptsResult SetSourcePathInPdbFile(string pathToAssembly, string scriptName, string scriptPath)
		{
			return new ScriptsResult();
		}
	}
}