using System.Reflection;

namespace ScriptingPlugin
{
	public enum ScriptsResult
	{
		AssemblyExists,
		CompilerError,
		PdbEditorError,
		GeneralError
	}

	public interface IScriptCompilerService
	{
		ScriptsResult TryCompile(string scriptName, string scriptPath, string script,out Assembly assembly);
		void SetPdbEditor(IPdbEditor pdbEditor);
	}
}