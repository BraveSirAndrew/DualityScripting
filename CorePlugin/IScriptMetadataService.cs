namespace ScriptingPlugin
{
	public interface IScriptMetadataService
	{
		void UpdateMetadata(string scriptPath);
		string GetMetafilePath(string scriptPath);
	}
}