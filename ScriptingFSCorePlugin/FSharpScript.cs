using System;
using Duality;
using Duality.Editor;
using ScriptingPlugin.FSharp;
using ScriptingPlugin.Resources;

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
			ScriptCompiler = ScriptingFsCorePlugin.FSharpScriptCompiler;
			base.OnLoaded();
		}
	}
}