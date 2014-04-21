using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Duality;
using Microsoft.FSharp.Compiler.SimpleSourceCodeServices;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace ScriptingPlugin
{
	public class FSharpScriptCompiler : IScriptCompiler
	{
		private List<string> _references = new List<string>();

		public FSharpScriptCompiler()
		{
			_references = new List<string> { "System.dll", "System.Core.dll", "System.Drawing.dll", "System.Xml.Linq", "Duality.dll", "FarseerDuality.dll", "plugins/ScriptingPlugin.core.dll", "OpenTK.dll", "System.Drawing" };
		}

		public Assembly Compile(string scriptName, string scriptPath, string script)
		{
			var scs = new SimpleSourceCodeServices();
			var outputAssemblyPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Path.GetTempFileName() + ".dll");
			var referencesAndScript = new List<string>();
			foreach (var reference in _references)
				referencesAndScript.Add(string.Format("--reference:{0}", reference));
			
			var options = new[] {"fsc.exe", "-o", outputAssemblyPath, "-a", "-g", "--lib:plugins"};

			referencesAndScript.Add(scriptPath);
			var completeOptions = options.Concat(referencesAndScript).ToArray();
			
			var errorsAndExitCode = scs.Compile(completeOptions);

			if (errorsAndExitCode.Item1.Any() || !File.Exists(outputAssemblyPath))
			{
				var text = errorsAndExitCode.Item1.Aggregate("", (current, ce) => current + ("\r\n" + ce));
				Log.Editor.WriteError("Error compiling script '{0}': {1}", scriptName, text);
				return null;
			}
			
			SetSourcePathInPdbFile(outputAssemblyPath, scriptName, scriptPath);
			return Assembly.LoadFile(outputAssemblyPath);
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