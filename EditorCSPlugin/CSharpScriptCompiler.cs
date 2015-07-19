using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Duality.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace ScriptingPlugin.CSharp
{
	public class CSharpScriptCompiler : IScriptCompiler
	{
		private CSharpCompilation _compilation;

		public CSharpScriptCompiler()
		{
			var executableReference = AssemblyMetadata.CreateFromImage(File.ReadAllBytes(typeof(object).Assembly.Location)).GetReference();
			_compilation = CSharpCompilation.Create("ScriptingAssembly")
				.AddReferences(new MetadataReference[]{ executableReference})
				.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			
			if (Directory.Exists(FileConstants.AssembliesDirectory) == false)
				Directory.CreateDirectory(FileConstants.AssembliesDirectory);
		}

		public IScriptCompilerResults Compile(string script, string sourceFilePath = null)
		{
			Guard.StringNotNullEmpty(script);
			return Compile(new[] { new CompilationUnit(script, sourceFilePath), });
		}

		public IScriptCompilerResults Compile(IEnumerable<CompilationUnit> scripts, string resultingAssemblyDirectory = null)
		{

			var assemblyDirectory = string.IsNullOrWhiteSpace(resultingAssemblyDirectory)
				? FileConstants.AssembliesDirectory
				: resultingAssemblyDirectory;
			if (!Directory.Exists(assemblyDirectory))
				Directory.CreateDirectory(assemblyDirectory);
			var tempFileName = Guid.NewGuid().ToString();
			var assemblyName = tempFileName + ".dll";
			var assemblyPath = Path.Combine(assemblyDirectory, assemblyName);

			var pdbName = tempFileName + ".pdb";
			var pdbPath = Path.Combine(assemblyDirectory, pdbName);


			using (var assemblyStream = new FileStream(assemblyPath, FileMode.Create))
			using (var pdbStream = new FileStream(pdbPath, FileMode.Create))
			{
				var syntaxTrees = new List<SyntaxTree>();
				foreach (var compilationUnit in scripts)
				{
					if (string.IsNullOrWhiteSpace(compilationUnit.Source))
						throw new ArgumentException("scriptsource");

					var sourcePath = string.IsNullOrEmpty(compilationUnit.SourceFilePath) ? "" : Path.GetFullPath(compilationUnit.SourceFilePath);

					var syntaxTree = CSharpSyntaxTree.ParseText(compilationUnit.Source);
					syntaxTree = CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree.GetRoot(), path: sourcePath, encoding: Encoding.UTF8);
					syntaxTrees.Add(syntaxTree);
				}

				var results = _compilation
						.AddSyntaxTrees(syntaxTrees)
						.WithAssemblyName(assemblyName)
						.Emit(assemblyStream, pdbStream, options: new EmitOptions(pdbFilePath: Path.Combine(assemblyDirectory, pdbName)));

				var errors = Enumerable.Empty<string>();
				if (!results.Success)
					errors = (from diagnostic in results.Diagnostics
						where diagnostic.Severity == DiagnosticSeverity.Error
							  select string.Format("{0} {3} {1} {2} ", diagnostic.Id, diagnostic.Location.GetLineSpan().StartLinePosition, diagnostic.GetMessage(), diagnostic.Location)).ToList();

				return new ScriptCompilerResults(results.Success, errors, assemblyPath);
			}
		}

		public void AddReference(string referenceAssembly)
		{
			if (string.IsNullOrEmpty(referenceAssembly) || !referenceAssembly.Contains("dll"))
				return;

			string filePath = null;
			if (File.Exists(referenceAssembly))
				filePath = Path.GetFullPath(referenceAssembly);
			else
			{
				var referencedAssemblyInAppDomain =
					AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == Path.GetFileNameWithoutExtension(referenceAssembly));
				
				if (referencedAssemblyInAppDomain != null)
				{
					filePath = referencedAssemblyInAppDomain.Location;
				}
				else
				{
					throw new ArgumentException(
						string.Format("Thre was a problem trying to find {0} either in the current folder or in the AppDomain, the solution is to try to load the missing assembly into the appdomain somehow",referenceAssembly));
				}
			}
			var executableReference = AssemblyMetadata.CreateFromImage(File.ReadAllBytes(filePath)).GetReference();
			_compilation = _compilation.AddReferences(new MetadataReference[]{ executableReference});
		}
	}
}