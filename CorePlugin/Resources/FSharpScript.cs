using System;
using Duality;
using Duality.Editor;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	[EditorHintCategory("Scripting")]
	[EditorHintImage("Resources", "fsharp")]
	public class FSharpScript : ScriptResourceBase
	{
		public new static string FileExt = ".FSharpScript" + Resource.FileExt;

		protected override void OnLoaded()
		{
			ScriptCompiler = ScriptingPluginCorePlugin.FSharpScriptCompiler;
			base.OnLoaded();
		}
	}
}