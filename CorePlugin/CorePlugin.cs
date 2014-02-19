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
        }
    }
}
