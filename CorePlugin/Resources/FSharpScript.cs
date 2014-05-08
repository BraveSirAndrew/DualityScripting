using System;
using Duality;
using Duality.Editor;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	[EditorHintCategory("Scripting")]
	public class FSharpScript : ScriptResourceBase
	{
		public new static string FileExt = FileConstants.FSharpExtension + Resource.FileExt;

		protected override void OnLoaded()
		{
			ScriptCompiler = ScriptingPluginCorePlugin.FSharpScriptCompiler;
			base.OnLoaded();
		}
	}
}