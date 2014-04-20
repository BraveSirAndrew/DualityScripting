using System;
using Duality;
using Duality.Editor;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	[EditorHintCategory("Scripting")]
	public class ScriptResource : ScriptResourceBase
	{
		protected override void OnLoaded()
		{
			ScriptCompiler = ScriptingPluginCorePlugin.CSharpScriptCompiler;
			FileExt = FileConstants.CSharpExtension + Resource.FileExt;
			base.OnLoaded();
		}
	}
}
