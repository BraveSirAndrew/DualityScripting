namespace ScriptingPlugin
{
	public struct CompilationUnit
	{
		public CompilationUnit(string source, string sourceFilePath) : this()
		{
			Source = source;
			SourceFilePath = sourceFilePath;
		}
		public string Source { get; private set; }
		public string SourceFilePath { get; private set; }
	}
}