using System;
using System.IO;
using Duality;

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

		protected override void InitPlugin()
		{
			base.InitPlugin();

			var cSharpScriptCompiler = new CSharpScriptCompiler();
			var fSharpScriptCompiler = new FSharpScriptCompiler();

			CSharpScriptCompiler = new ScriptCompilerService(cSharpScriptCompiler, new NullPdbEditor());
			FSharpScriptCompiler = new ScriptCompilerService(fSharpScriptCompiler, new NullPdbEditor());

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
		}
	}
}
