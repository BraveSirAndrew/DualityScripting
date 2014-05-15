using System;
using System.Linq;
using Duality;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace ScriptingPlugin
{
	public class PdbEditor : IPdbEditor
	{
		public CompilerResult SetSourcePathInPdbFile(string pathToAssembly, string scriptName, string scriptPath)
		{
			try
			{
				var readerParameters = new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = new PdbReaderProvider() };
				var assemblyDef = AssemblyDefinition.ReadAssembly(pathToAssembly, readerParameters);

				var moduleDefinition = assemblyDef.Modules[0];
				moduleDefinition.ReadSymbols();
				var type = moduleDefinition.Types.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "DualityScript");

				if (type == null)
				{
					Log.Editor.WriteError("Script file '{0}' has to contain a class that derives from DualityScript.", scriptName);
					return CompilerResult.PdbEditorError;
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
			catch (Exception exception)
			{
				Log.Editor.WriteError("There was a problem editing the pdb file after compiling the script {0}, {1} Error: {2} {1} StackTrace: {3}", scriptName,Environment.NewLine, exception.Message, exception.StackTrace);
				return CompilerResult.PdbEditorError;
			}

			return CompilerResult.AssemblyExists;
		}
	}
}