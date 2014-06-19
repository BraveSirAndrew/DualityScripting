namespace ScriptingPlugin.Editor
{
	public interface IScriptProjectEditor
	{
		void AddScriptToProject(string scriptPath, string scriptFileName, string projectPath);
		void RemoveScriptFromProject(string oldContentName, string extension, string projectPath);
	}
}