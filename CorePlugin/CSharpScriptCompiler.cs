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

		public IScriptCompilerResults Compile(string script)
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
			return new CSharpScriptCompilerResults(results);
		}

		public void AddReference(string referenceAssembly)
		{
			if (string.IsNullOrEmpty(referenceAssembly) || !referenceAssembly.Contains("dll"))
				return;
			
			_references.Add(referenceAssembly);
		}
	}
}