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
	    public static ScriptCompiler ScriptCompiler { get; set; }

        protected override void InitPlugin()
        {
            base.InitPlugin();

            ScriptCompiler = new ScriptCompiler();

	        foreach (var file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Plugins"), "*.core.dll"))
	        {
		        ScriptCompiler.AddReference("Plugins//" + Path.GetFileName(file));
	        }
        }
    }
}
