using System.Collections.Generic;
using System.Reflection;

namespace ScriptingPlugin
{
	public class CSharpScriptCompilerResults : IScriptCompilerResults
	{
		private readonly bool _success;
		private readonly IEnumerable<string> _errors;
		private readonly string _assemblyPath;

		public CSharpScriptCompilerResults(bool success, IEnumerable<string> errors , string assemblyPath)
		{
			_success = success;
			_errors = errors;
			_assemblyPath = assemblyPath;
		}

		public IEnumerable<string> Errors
		{
			get { return _errors; }
		}

		public Assembly CompiledAssembly
		{
			get
			{
				return _success ? Assembly.LoadFrom(_assemblyPath) : null;
			}
		}

		public string PathToAssembly
		{
			get { return _assemblyPath; }
		}
	}
}