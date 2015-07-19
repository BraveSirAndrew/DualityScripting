using System;
using Duality;
using Duality.Editor;
using ScriptingPlugin.FSharp;

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
#if DEBUG
			ScriptCompiler = ScriptingPluginCorePlugin.FSharpScriptCompiler;
			base.OnLoaded();
		}
	}
}