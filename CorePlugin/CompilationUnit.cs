namespace ScriptingPlugin
{
	public struct CompilationUnit
	{
		public CompilationUnit(string source, string sourceFilePath=null) : this()
		{
			Source = source;
			SourceFilePath = sourceFilePath;
		}
		public string Source { get; set; }
		public string SourceFilePath { get; set; }
	}
}