using System;
using System.IO;
using Duality;

namespace ScriptingPlugin.FSharp
{
	public class ScriptingFsCorePlugin : CorePlugin
	{
		public static IScriptCompilerService FSharpScriptCompiler { get; set; }

		protected override void InitPlugin()
		{
			base.InitPlugin();

			var fSharpScriptCompiler = new FSharpScriptCompiler();
			FSharpScriptCompiler = new ScriptCompilerService(fSharpScriptCompiler);
			foreach (var file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Plugins"), "*.core.dll"))
			{
				fSharpScriptCompiler.AddReference("Plugins//" + Path.GetFileName(file));
			}

			if (File.Exists(ScriptingPluginCorePlugin.ReferenceAssembliesFile))
			{
				var assemblies = File.ReadAllText(ScriptingPluginCorePlugin.ReferenceAssembliesFile).Split('\n');

				foreach (var assembly in assemblies)
				{
					if (!assembly.EndsWith("System.Runtime.dll", StringComparison.CurrentCultureIgnoreCase))
						fSharpScriptCompiler.AddReference(assembly);
				}
			}
			ScriptingPluginCorePlugin.ExcludeAssembliesFromTypeSearch();
			ScriptingPluginCorePlugin.FSharpScriptCompiler = FSharpScriptCompiler;
		}

	}
}
