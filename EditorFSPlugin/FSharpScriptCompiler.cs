using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Duality;
using Duality.Helpers;
using Microsoft.FSharp.Compiler.SimpleSourceCodeServices;

namespace ScriptingPlugin.FSharp
{
	public class FSharpScriptCompiler : IScriptCompiler
	{
		private readonly HashSet<string> _references = new HashSet<string>();
		private SimpleSourceCodeServices _sourceCodeServices;

		public HashSet<string> References { get { return _references; } }

		public FSharpScriptCompiler()
		{
			try
			{
				_sourceCodeServices = new SimpleSourceCodeServices();

				if (Directory.Exists(FileConstants.AssembliesDirectory) == false)
					Directory.CreateDirectory(FileConstants.AssembliesDirectory);
			}
			catch (Exception exception)
			{
				Log.Editor.WriteWarning("Could not start compiler services for FSharp {0} \n {1}", exception.Message, exception.StackTrace);
			}
		}

		public IScriptCompilerResults Compile(IEnumerable<CompilationUnit> compilationUnits, string resultingAssemblyDirectory = null)
		{
			_sourceCodeServices = _sourceCodeServices ?? new SimpleSourceCodeServices();
			var assemblyName = "FS-" + Guid.NewGuid() + ".dll";
			var assemblyDirectory = string.IsNullOrWhiteSpace(resultingAssemblyDirectory)
				? Path.Combine(Environment.CurrentDirectory, FileConstants.AssembliesDirectory)
				: resultingAssemblyDirectory;
			if (!Directory.Exists(assemblyDirectory))
				Directory.CreateDirectory(assemblyDirectory);
			var outputAssemblyPath = Path.Combine(assemblyDirectory, assemblyName);
			var referencesAndScript = new List<string>();

			foreach (var reference in _references)
			{
				if (!string.IsNullOrWhiteSpace(reference))
					referencesAndScript.Add(string.Format("--reference:{0}", reference));
			}

			var enumerable = compilationUnits as CompilationUnit[] ?? compilationUnits.ToArray();
			foreach (var compilationUnit in enumerable)
			{
				if (string.IsNullOrWhiteSpace(compilationUnit.Source))
					throw new ArgumentException("scriptsource");
				if (string.IsNullOrWhiteSpace(compilationUnit.SourceFilePath))
					throw new ArgumentException("scriptsourceFilePath");

				referencesAndScript.Add(compilationUnit.SourceFilePath);
			}
			var options = new[] { "fsc.exe", "-o", outputAssemblyPath, "-a", "--debug+", "--optimize-", "--noframework" };
			string[] completeOptions = options.Concat(referencesAndScript).ToArray();
			var errorsAndExitCode = _sourceCodeServices.Compile(completeOptions);

			var errorsWithoutWarnings = errorsAndExitCode.Item1.Where(x => x.Severity.IsError);
			var errors = errorsWithoutWarnings
					.Select(x => string.Format("{0} {1} {2} ", x.StartLineAlternate, x.StartColumn, x.Message))
					.ToArray();
			return new ScriptCompilerResults(!errors.Any(), errors, outputAssemblyPath);
		}

		public IScriptCompilerResults Compile(string script, string sourceFilePath)
		{
			Guard.StringNotNullEmpty(script);
			return Compile(new[] { new CompilationUnit(script, sourceFilePath) });
		}

		public void AddReference(string referenceAssembly)
		{
			if (string.IsNullOrWhiteSpace(referenceAssembly))
				return;
			if (!referenceAssembly.EndsWith("System.Runtime.dll", StringComparison.CurrentCultureIgnoreCase))
				if (!ExistsCompareByFileName(_references, referenceAssembly))
				{
					_references.Add(referenceAssembly.Trim());
				}
		}

		private static bool ExistsCompareByFileName(HashSet<string> references, string referenceAssembly)
		{
			var fileName = Path.GetFileName(referenceAssembly);
			if (string.IsNullOrWhiteSpace(fileName))
				return false;

			return references.Any(x => x == fileName.ToLower());
		}
	}
}