using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Duality.Helpers;
using Microsoft.CSharp;

namespace ScriptingPlugin
{
	public class CSharpScriptCompiler : IScriptCompiler
	{
		private readonly CSharpCodeProvider _provider;
		private readonly List<string> _references = new List<string>();

		public CSharpScriptCompiler()
		{
			_provider = new CSharpCodeProvider();
		}

		public ScriptCompilerResults Compile(string script)
		{
			Guard.StringNotNullEmpty(script);

			var compilerParams = new CompilerParameters
			{
				GenerateInMemory = false,
				TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true),
				IncludeDebugInformation = true,
				TreatWarningsAsErrors = false,
				GenerateExecutable = false,
				CompilerOptions = " /debug:pdbonly"
			};
			compilerParams.ReferencedAssemblies.AddRange(_references.ToArray());
			var results = _provider.CompileAssemblyFromSource(compilerParams, script);
			var sb = (from CompilerError error in results.Errors select string.Format("{0} {1} {2} ", error.ErrorNumber, error.Line, error.ErrorText)).ToList();
			return new ScriptCompilerResults(sb, results.CompiledAssembly, results.PathToAssembly);
		}

		public void AddReference(string referenceAssembly)
		{
			if (string.IsNullOrEmpty(referenceAssembly) || !referenceAssembly.Contains("dll"))
				return;
			
			_references.Add(referenceAssembly);
		}
	}
}