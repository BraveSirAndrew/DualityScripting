using System;
using System.IO;
using Duality;


namespace ScriptingPlugin.CSharp
{
	public class ScriptingCsCorePlugin : CorePlugin
	{
		public static IScriptCompilerService CSharpScriptCompiler { get; set; }
		protected override void InitPlugin()
		{
			base.InitPlugin();

			var cSharpScriptCompiler = new CSharpScriptCompiler();
			CSharpScriptCompiler = new ScriptCompilerService(cSharpScriptCompiler);

			foreach (var file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Plugins"), "*.core.dll"))
			{
				cSharpScriptCompiler.AddReference("Plugins//" + Path.GetFileName(file));
			}

			if (File.Exists(ScriptingPluginCorePlugin.ReferenceAssembliesFile))
			{
				var assemblies = File.ReadAllText(ScriptingPluginCorePlugin.ReferenceAssembliesFile).Split('\n');

				foreach (var assembly in assemblies)
					cSharpScriptCompiler.AddReference(assembly);
			}
			
			ScriptingPluginCorePlugin.ExcludeAssembliesFromTypeSearch();
			ScriptingPluginCorePlugin.CSharpScriptCompiler = CSharpScriptCompiler;
		}
	}
}
