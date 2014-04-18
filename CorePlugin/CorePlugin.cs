using System;
using System.IO;
using Duality;

namespace ScriptingPlugin
{
	/// <summary>
	/// Defines a Duality core plugin.
	/// </summary>
    public class ScriptingPluginCorePlugin : CorePlugin
    {
		public const string CSharpScriptExtension = ".cs";
		public const string DataScripts = "Data\\Scripts\\";
		private const string ReferenceAssembliesFile = "ScriptReferences.txt";

		public static CSharpScriptCompiler CSharpScriptCompiler { get; set; }
		
		public static FSharpScriptCompiler FSharpScriptCompiler { get; set; }

        protected override void InitPlugin()
        {
            base.InitPlugin();

            CSharpScriptCompiler = new CSharpScriptCompiler();
			FSharpScriptCompiler = new FSharpScriptCompiler();

	        foreach (var file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Plugins"), "*.core.dll"))
	        {
		        CSharpScriptCompiler.AddReference("Plugins//" + Path.GetFileName(file));
		        FSharpScriptCompiler.AddReference("Plugins//" + Path.GetFileName(file));
	        }

	        if (File.Exists(ReferenceAssembliesFile))
	        {
		        var assemblies = File.ReadAllText(ReferenceAssembliesFile).Split('\n');

		        foreach (var assembly in assemblies)
		        {
			        CSharpScriptCompiler.AddReference(assembly);
			        FSharpScriptCompiler.AddReference(assembly);
		        }
	        }
        }
    }
}
