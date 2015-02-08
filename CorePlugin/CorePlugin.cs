using System;
using System.IO;
using System.IO.Abstractions;
using Duality;

namespace ScriptingPlugin
{
	public class ScriptingPluginCorePlugin : CorePlugin
	{
		public const string DataScripts = "Data\\Scripts\\";
		private const string ReferenceAssembliesFile = "ScriptReferences.txt";

		public static IScriptCompilerService CSharpScriptCompiler { get; set; }
		public static IScriptCompilerService FSharpScriptCompiler { get; set; }
		public static IScriptMetadataService ScriptMetadataService { get; set; }

		protected override void InitPlugin()
		{
			base.InitPlugin();

			var cSharpScriptCompiler = new CSharpScriptCompiler();
			var fSharpScriptCompiler = new FSharpScriptCompiler();

			CSharpScriptCompiler = new ScriptCompilerService(cSharpScriptCompiler, new NullPdbEditor());
			FSharpScriptCompiler = new ScriptCompilerService(fSharpScriptCompiler, new NullPdbEditor());

			ScriptMetadataService = new ScriptMetadataService(new FileSystem());

			foreach (var file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Plugins"), "*.core.dll"))
			{
				cSharpScriptCompiler.AddReference("Plugins//" + Path.GetFileName(file));
				fSharpScriptCompiler.AddReference("Plugins//" + Path.GetFileName(file));
			}

			if (File.Exists(ReferenceAssembliesFile))
			{
				var assemblies = File.ReadAllText(ReferenceAssembliesFile).Split('\n');

				foreach (var assembly in assemblies)
				{
					cSharpScriptCompiler.AddReference(assembly);
					if (!assembly.EndsWith("System.Runtime.dll",StringComparison.CurrentCultureIgnoreCase))
						fSharpScriptCompiler.AddReference(assembly);
				}
			}

			ExcludeAssembliesFromTypeSearch();
		}

		private static void ExcludeAssembliesFromTypeSearch()
		{
			ReflectionHelper.ExcludeFromTypeSearches(new[]
			{
				"FSharp.Compiler.Service",
				"Microsoft.CodeAnalysis.CSharp",
				"Microsoft.CodeAnalysis",
				"System.Reflection.Metadata",
				"System.Collections.Immutable",
				"Mono.Cecil",
				"Mono.Cecil.Pdb"
			});
		}
	}
}