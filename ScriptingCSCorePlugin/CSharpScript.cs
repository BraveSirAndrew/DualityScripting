using System;
using Duality;
using Duality.Editor;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.CSharp
{
	[Serializable]
	[EditorHintCategory("Scripting")]
	[EditorHintImage("Resources", "csharp")]
	public class CSharpScript : ScriptResourceBase
	{
		public new static string FileExt = ".CSharpScript" + Resource.FileExt;

		protected override void OnLoaded()
		{
			ScriptCompiler = ScriptingCsCorePlugin.CSharpScriptCompiler;
			base.OnLoaded();
		}
	}
}