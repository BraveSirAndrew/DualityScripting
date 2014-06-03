using System.Collections.Generic;
using System.Reflection;

namespace ScriptingPlugin
{
	public class ScriptCompilerResults
	{
		private readonly IEnumerable<string> _errors;
		private readonly Assembly _assembly;
		private readonly string _pathToAssembly;

		public ScriptCompilerResults(IEnumerable<string> errors, Assembly assembly, string pathToAssembly)
		{
			_errors = errors;
			_assembly = assembly;
			_pathToAssembly = pathToAssembly;
		}

		public IEnumerable<string> Errors { get { return _errors; } }

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