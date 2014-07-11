using System;
using System.IO;
using System.Linq;
using Duality.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ScriptingPlugin
{
	public class CSharpScriptCompiler : IScriptCompiler
	{
		private CSharpCompilation _compilation;

		public CSharpScriptCompiler()
		{
			_compilation = CSharpCompilation.Create("ScriptingAssembly",
				references: new[] { new MetadataFileReference(typeof(object).Assembly.Location) },
				options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, 
					debugInformationKind: DebugInformationKind.Full));

			if (Directory.Exists(FileConstants.AssembliesDirectory) == false)
				Directory.CreateDirectory(FileConstants.AssembliesDirectory);
		}

		public IScriptCompilerResults Compile(string script, string sourceFilePath = null)
		{
			Guard.StringNotNullEmpty(script);

			var tempFileName = Guid.NewGuid().ToString();
			var assemblyName = tempFileName + ".dll";
			var assemblyPath = Path.Combine(FileConstants.AssembliesDirectory, assemblyName);

			var pdbName = tempFileName + ".pdb";
			var pdbPath = Path.Combine(FileConstants.AssembliesDirectory, pdbName);

			using (var assemblyStream = new FileStream(assemblyPath, FileMode.Create))
			using (var pdbStream = new FileStream(pdbPath, FileMode.Create))
			{
				var sourcePath = string.IsNullOrEmpty(sourceFilePath) ? "" : Path.GetFullPath(sourceFilePath);

				var syntaxTree = CSharpSyntaxTree.ParseText(script);
				syntaxTree = CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree.GetRoot(), sourcePath);

				var results = _compilation
					.AddSyntaxTrees(syntaxTree)
					.WithAssemblyName(assemblyName)
					.Emit(assemblyStream, pdbStream: pdbStream, pdbFileName: pdbName);
				return new CSharpScriptCompilerResults(results, assemblyPath);
			}
		}

		public void AddReference(string referenceAssembly)
		{
			if (string.IsNullOrEmpty(referenceAssembly) || !referenceAssembly.Contains("dll"))
				return;

			string filePath;
			if (File.Exists(referenceAssembly))
				filePath = Path.GetFullPath(referenceAssembly);
			else
				filePath = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == Path.GetFileNameWithoutExtension(referenceAssembly)).Location;

			_compilation = _compilation.AddReferences(new MetadataFileReference(filePath));
		}
	}
}