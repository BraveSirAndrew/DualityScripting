using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Duality;
using Duality.Helpers;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Compiler.SimpleSourceCodeServices;

namespace ScriptingPlugin
{
	public class FSharpScriptCompiler : IScriptCompiler
	{
		private List<string> _references = new List<string>();
		private readonly SimpleSourceCodeServices _sourceCodeServices;

		public FSharpScriptCompiler()
		{

			try
			{
				_sourceCodeServices = new SimpleSourceCodeServices();
			}
			catch (Exception exception)
			{
				Log.Editor.WriteWarning("Could not start compiler services for FSharp {0} \n {1}", exception.Message, exception.StackTrace);
			}
		}

		public ScriptCompilerResults Compile(string script)
		{
			Guard.StringNotNullEmpty(script);
			Guard.NotNull(_sourceCodeServices);
			var outputAssemblyPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Path.GetTempFileName() + ".dll");
			var referencesAndScript = new List<string>();

			foreach (var reference in _references)
			{
				if(!string.IsNullOrWhiteSpace(reference))
					referencesAndScript.Add(string.Format("--reference:{0}", reference));
			}

			var options = new[] { "fsc.exe", "-o", outputAssemblyPath, "-a", "-g", "--noframework" };

			var tempScriptPath = "";
			CompilerResults compilerResults;

			tempScriptPath = Path.GetTempFileName().Replace("tmp", "fs");
			File.WriteAllText(tempScriptPath, script);

			referencesAndScript.Add(tempScriptPath);
			var completeOptions = options.Concat(referencesAndScript).ToArray();
			var errorsAndExitCode = _sourceCodeServices.Compile(completeOptions);
			Assembly assembly = null;
			try
			{
				assembly = Assembly.LoadFile(outputAssemblyPath);
			}
			catch (Exception e)
			{
				Log.Editor.WriteWarning("{0}: Couldn't load assembly file", GetType().Name);
			}
			finally
			{
				if (File.Exists(tempScriptPath))
					File.Delete(tempScriptPath);
			}
			var errors = errorsAndExitCode.Item1.Select(x => string.Format("{0} {1} {2} ", x.Severity, x.StartLineAlternate, x.Message));
			return new ScriptCompilerResults(errors, assembly, outputAssemblyPath);
		}

		public void AddReference(string referenceAssembly)
		{
			if(string.IsNullOrWhiteSpace(referenceAssembly))
				return;
			
			_references.Add(referenceAssembly.Trim());
		}
	}
}