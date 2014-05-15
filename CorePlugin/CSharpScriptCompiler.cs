using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Duality.Helpers;
using Microsoft.CSharp;

namespace ScriptingPlugin
{
	public class CSharpScriptCompiler : IScriptCompiler
	{
		private CSharpCodeProvider _provider;
		private List<string> _references = new List<string>();

		public CSharpScriptCompiler()
		{
			_provider = new CSharpCodeProvider();
		}

		public CompilerResults Compile(string script)
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
			return _provider.CompileAssemblyFromSource(compilerParams, script);
		}

		public void AddReference(string referenceAssembly)
		{
			if (string.IsNullOrEmpty(referenceAssembly) || !referenceAssembly.Contains("dll"))
				return;
			
			_references.Add(referenceAssembly);
		}
	}
}