using System;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using Duality;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin
{
	public class ScriptingPluginCorePlugin : CorePlugin
	{
		public const string CSharpScriptExtension = ".cs";
		public const string FSharpScriptExtension = ".fs";
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
					fSharpScriptCompiler.AddReference(assembly);
				}
			}

			ExcludeAssembliesFromTypeSearch();
		}

		private static void ExcludeAssembliesFromTypeSearch()
		{
			ReflectionHelper.ExcludeFromTypeSearches(new[]
			{
				LoadPluginAssembly("FSharp.Compiler.Service.dll"),
				LoadPluginAssembly("Microsoft.CodeAnalysis.CSharp.dll"),
				LoadPluginAssembly("Microsoft.CodeAnalysis.dll"),
				LoadPluginAssembly("System.Reflection.Metadata.dll"),
				LoadPluginAssembly("System.Collections.Immutable.dll"),
				LoadPluginAssembly("Mono.Cecil.dll"),
				LoadPluginAssembly("Mono.Cecil.Pdb.dll")
			});
		}

		private static Assembly LoadPluginAssembly(string filename)
		{
			try
			{
				return Assembly.LoadFrom(Path.Combine(DualityApp.PluginDirectory, filename));
			}
			catch(Exception e)
			{
				Log.Game.WriteWarning("Error while loading assembly {0}. {1}", filename, e.Message);
				return null;
			}
		}
	}
}