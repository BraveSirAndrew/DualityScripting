using System;
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
		private SimpleSourceCodeServices _sourceCodeServices;

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
			var outputAssemblyPath = Path.Combine(Environment.CurrentDirectory, FileConstants.AssembliesDirectory, assemblyName);

			var referencesAndScript = new List<string>();

			foreach (var reference in _references)
			{
				if (!string.IsNullOrWhiteSpace(reference))
					referencesAndScript.Add(string.Format("--reference:{0}", reference));
			}

			string[] completeOptions = null;
			var deleteTempFiles = new List<string>();
			foreach (var compilationUnit in compilationUnits)
			{
				if(string.IsNullOrWhiteSpace(compilationUnit.Source))
					throw new ArgumentException("scriptsource");

				var tempScriptPath = Path.GetTempFileName().Replace("tmp", "fs");
				File.WriteAllText(tempScriptPath, compilationUnit.Source);
				referencesAndScript.Add(tempScriptPath);
				deleteTempFiles.Add(tempScriptPath);
				
			}
			var options = new[] { "fsc.exe", "-o", outputAssemblyPath, "-a", "-g", "--noframework" };
			completeOptions = options.Concat(referencesAndScript).ToArray();
			var errorsAndExitCode = _sourceCodeServices.Compile(completeOptions);
			
			Assembly assembly = null;
			try
			{
				assembly = Assembly.LoadFile(outputAssemblyPath);
			}
			catch (Exception e)
			{
				Log.Editor.WriteWarning("{0}: Couldn't load assembly file. {1} ", GetType().Name, e.Message);
			}
			finally
			{
				foreach (var tempScriptPath in deleteTempFiles)
				{
					if (File.Exists(tempScriptPath))
						File.Delete(tempScriptPath);
				}
			}
			var eerr = errorsAndExitCode.Item1.Where(x => x.Severity.Equals(Severity.Error));
			var errors = eerr.Select(x => string.Format("{0} {1} {2} ", x.Severity, x.StartLineAlternate, x.Message));
			return new FSharpScriptCompilerResults(errors, assembly, outputAssemblyPath);
		}

		public IScriptCompilerResults Compile(string script, string sourceFilePath = null)
		{
			Guard.StringNotNullEmpty(script);
			return Compile(new []{new CompilationUnit(script, sourceFilePath)});

		}

		public void AddReference(string referenceAssembly)
		{
			if(string.IsNullOrWhiteSpace(referenceAssembly))
				return;
			
			_references.Add(referenceAssembly.Trim());
		}
	}
}