using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace ScriptingPlugin
{
	public class CSharpScriptCompilerResults : IScriptCompilerResults
	{
		private readonly EmitResult _results;
		private readonly string _assemblyPath;

		public CSharpScriptCompilerResults(EmitResult results, string assemblyPath)
		{
			_results = results;
			_assemblyPath = assemblyPath;
		}

		public IEnumerable<string> Errors
		{
			get
			{
				if (_results.Success)
					return Enumerable.Empty<string>();

				return (from diagnostic in _results.Diagnostics 
						where diagnostic.Severity == DiagnosticSeverity.Error 
						select string.Format("{0} {1} {2} ", diagnostic.Id, diagnostic.Location.GetLineSpan().StartLinePosition, diagnostic.GetMessage())).ToList();
			}
		}

		public Assembly CompiledAssembly
		{
			get
			{
				return _results.Success ? Assembly.LoadFrom(_assemblyPath) : null;
			}
		}

		public string PathToAssembly
		{
			get { return _assemblyPath; }
		}
	}
}