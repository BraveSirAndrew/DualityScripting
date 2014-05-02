using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duality;
using Duality.Helpers;
using Microsoft.CSharp;

namespace ScriptingPlugin
{
	public class CSharpScriptCompiler : IScriptCompiler
	{
		private readonly List<string> _references = new List<string>();

		public CompilerResult TryCompile(string scriptName, string scriptPath, string script, out Assembly assembly)
		{
			assembly = null;
			try
			{
				var compilerResult = Compile(script);

				if (compilerResult.Errors.HasErrors)
				{
					var text = compilerResult.Errors.Cast<CompilerError>().Aggregate("", (current, compilerError) => current + (Environment.NewLine + compilerError));
					Log.Editor.WriteError("Error compiling script '{0}': {1}", scriptName, text);
					return CompilerResult.CompilerError;
				}				
				assembly = compilerResult.CompiledAssembly;
				return CompilerResult.AssemblyExists;
			}
			catch (Exception exception)
			{
				Log.Editor.WriteError("Could not compile script {0} error {1}", scriptName, exception);
				return CompilerResult.GeneralError;
			}
		}

		private CompilerResults Compile(string script)
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

				var provider = new CSharpCodeProvider();
				return provider.CompileAssemblyFromSource(compilerParams, script);
		}

		public void AddReference(string referenceAssembly)
		{
			if (string.IsNullOrEmpty(referenceAssembly) || !referenceAssembly.Contains("dll"))
				return;

			_references.Add(referenceAssembly);
		}
	}
}