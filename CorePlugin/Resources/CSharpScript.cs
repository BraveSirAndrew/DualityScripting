using System;
using Duality;
using Duality.Editor;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	[EditorHintCategory("Scripting")]
	public class CSharpScript : ScriptResourceBase
	{
		public new static string FileExt = FileConstants.CSharpExtension + Resource.FileExt;

		protected override void OnLoaded()
		{
			ScriptCompiler = ScriptingPluginCorePlugin.CSharpScriptCompiler;
			base.OnLoaded();
		}
	}
}
