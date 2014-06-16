using System.Collections.Generic;
using System.Reflection;

namespace ScriptingPlugin
{
	public class FSharpScriptCompilerResults : IScriptCompilerResults
	{
		private IEnumerable<string> _errors;
		private Assembly _assembly;
		private string _pathToAssembly;

		public FSharpScriptCompilerResults(IEnumerable<string> errors, Assembly assembly, string pathToAssembly)
		{
			_errors = errors;
			_assembly = assembly;
			_pathToAssembly = pathToAssembly;
		}
		
		public IEnumerable<string> Errors
		{
			get { return _errors; }
		}

		public Assembly CompiledAssembly
		{
			get { return _assembly; }
		}

		public string PathToAssembly
		{
			get { return _pathToAssembly; }
		}
	}
}