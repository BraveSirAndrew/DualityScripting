using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duality;
using Microsoft.CSharp;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace ScriptingPlugin
{
	public class CSharpScriptCompiler : IScriptCompiler
	{
		private List<string> _references = new List<string>();

		public CSharpScriptCompiler()
		{
			//TODO: refactor to scan dependecies 
			_references = new List<string> { "System.dll", "System.Core.dll", "Duality.dll", "FarseerOpenTK.dll", "plugins/ScriptingPlugin.core.dll", "OpenTK.dll" };
		}

		public Assembly Compile(string scriptName, string scriptPath, string script)
		{
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
			var compile = provider.CompileAssemblyFromSource(compilerParams, script);

			if (compile.Errors.HasErrors == true)
			{
				var text = compile.Errors.Cast<CompilerError>().Aggregate("", (current, ce) => current + ("\r\n" + ce));
				Log.Editor.WriteError("Error compiling script '{0}': {1}", scriptName, text);
				return null;
			}

			SetSourcePathInPdbFile(compile, scriptName, scriptPath);
			return compile.CompiledAssembly;
		}

		public void AddReference(string referenceAssembly)
		{
			_references.Add(referenceAssembly);
		}

		private void SetSourcePathInPdbFile(CompilerResults compile, string scriptName, string scriptPath)
		{
			var readerParameters = new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = new PdbReaderProvider() };
			var assemblyDef = AssemblyDefinition.ReadAssembly(compile.PathToAssembly, readerParameters);

			var moduleDefinition = assemblyDef.Modules[0];
			moduleDefinition.ReadSymbols();
			var type = moduleDefinition.Types.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "DualityScript");

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
			assemblyDef.Write(compile.PathToAssembly, writerParameters);
		}
	}
}