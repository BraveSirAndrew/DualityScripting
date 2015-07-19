using System;
using System.IO;
using System.IO.Abstractions;
using Duality;

namespace ScriptingPlugin
{
	public class ScriptingPluginCorePlugin : CorePlugin
	{
		public const string DataScripts = "Data\\Scripts\\";
		public const string ReferenceAssembliesFile = "ScriptReferences.txt";

		public static IScriptMetadataService ScriptMetadataService { get; set; }
<<<<<<< HEAD
=======
		public static IScriptCompilerService CSharpScriptCompiler { get; set; }
		public static IScriptCompilerService FSharpScriptCompiler { get; set; }
>>>>>>> 57ab38a... Separating ScriptCompiler from script

		protected override void InitPlugin()
		{
			base.InitPlugin();

			ScriptMetadataService = new ScriptMetadataService(new FileSystem());
		}

		public static void ExcludeAssembliesFromTypeSearch()
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