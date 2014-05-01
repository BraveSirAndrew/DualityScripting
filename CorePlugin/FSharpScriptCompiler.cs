using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Duality;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Compiler.SimpleSourceCodeServices;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace ScriptingPlugin
{
	public class FSharpScriptCompiler : IScriptCompiler
	{
		private List<string> _references = new List<string>();

		public CompilerResult TryCompile(string scriptName, string scriptPath, string script, out Assembly assembly)
		{
			assembly = null;
			try
			{
				var compilerResult = Compile(scriptName, scriptPath, script);
				if (compilerResult.Item1.Any() || !File.Exists(compilerResult.Item2))
				{
					var text = compilerResult.Item1.Aggregate("", (current, ce) => current + (Environment.NewLine + ce));
					Log.Editor.WriteError("Error compiling script '{0}': {1}", scriptName, text);
					return CompilerResult.CompilerError;
				}
				
				if (compilerResult.Item2 != null)
					assembly = Assembly.LoadFile(compilerResult.Item2);
				return CompilerResult.AssemblyExists;
			}
			catch (Exception exception)
			{
				Log.Editor.WriteError("Could not compile script {0} error {1}", scriptName, exception);
				return CompilerResult.GeneralError;
			}
		}

		private Tuple<ErrorInfo[], string> Compile(string scriptName, string scriptPath, string script)
		{
			var scs = new SimpleSourceCodeServices();
			var outputAssemblyPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Path.GetTempFileName() + ".dll");
			var referencesAndScript = new List<string>();
			foreach (var reference in _references)
				referencesAndScript.Add(string.Format("--reference:{0}", reference));

			var options = new[] { "fsc.exe", "-o", outputAssemblyPath, "-a", "-g", "--lib:plugins", "--noframework" };

			referencesAndScript.Add(scriptPath);
			var completeOptions = options.Concat(referencesAndScript).ToArray();
			var errorsAndExitCode = scs.Compile(completeOptions);
			return Tuple.Create(errorsAndExitCode.Item1, outputAssemblyPath);
		}

		public void AddReference(string referenceAssembly)
		{
			_references.Add(referenceAssembly);
		}

		private void SetSourcePathInPdbFile(string pathToAssembly, string scriptName, string scriptPath)
		{
			var readerParameters = new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = new PdbReaderProvider() };
			var assemblyDef = AssemblyDefinition.ReadAssembly(pathToAssembly, readerParameters);

			var moduleDefinition = assemblyDef.Modules[0];
			moduleDefinition.ReadSymbols();
			TypeDefinition type = null;
			foreach (TypeDefinition t in moduleDefinition.Types)
			{
				if (t.BaseType != null && t.BaseType.FullName == typeof (DualityScript).FullName)
				{
					type = t;
					break;
				}

				if (t.HasNestedTypes)
				{
					foreach (var nested in t.NestedTypes)
					{
						if (nested.BaseType != null && nested.BaseType.FullName == typeof(DualityScript).FullName)
						{
							type = nested;
							break;
						}
					}
				}
			}

			if (type == null)
			{
				Log.Editor.WriteError("Script file '{0}' has to contain a class that derives from DualityScript.", scriptName);
				return;
			}

			foreach (var method in type.Methods)
			{
				var instruction = method.Body.Instructions.FirstOrDefault(i => i.SequencePoint != null);
				if (instruction == null)
					continue;

				instruction.SequencePoint.Document.Url = scriptPath;
			}

			var writerParameters = new WriterParameters { WriteSymbols = true, SymbolWriterProvider = new PdbWriterProvider() };
			assemblyDef.Write(pathToAssembly, writerParameters);
		}

	}
}