using System.Collections.Generic;
using System.Reflection;

namespace ScriptingPlugin
{
	public interface IScriptCompilerResults
	{
		IEnumerable<string> Errors { get; }
		Assembly CompiledAssembly { get; }
		string PathToAssembly { get; }
	}
}