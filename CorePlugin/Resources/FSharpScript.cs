using System;
using Duality;
using Duality.Editor;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	[EditorHintCategory("Scripting")]
	public class FSharpScript : ScriptResourceBase
	{

		protected override void OnLoaded()
		{
			ScriptCompiler = ScriptingPluginCorePlugin.FSharpScriptCompiler;
			
			FileExt = FileConstants.FSharpExtension + Resource.FileExt;
			base.OnLoaded();
		}
	}
}