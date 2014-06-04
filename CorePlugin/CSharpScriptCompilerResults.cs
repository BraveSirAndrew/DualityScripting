using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ScriptingPlugin
{
	public class CSharpScriptCompilerResults : IScriptCompilerResults
	{
		private readonly CompilerResults _results;
		
		public CSharpScriptCompilerResults(CompilerResults results)
		{
			_results = results;
		}

		public IEnumerable<string> Errors
		{
			get
			{
				return from CompilerError error in _results.Errors 
					   select string.Format("{0} {1} {2} ", error.ErrorNumber, error.Line, error.ErrorText);
			}
		}

		public Assembly CompiledAssembly
		{
			get
			{
				return _results.Errors.HasErrors == false ? _results.CompiledAssembly : null;
			}
		}

		public string PathToAssembly
		{
			get { return _results.PathToAssembly; }
		}
	}
}