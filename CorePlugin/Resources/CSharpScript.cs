using System;
using Duality;
using Duality.Editor;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	[EditorHintCategory("Scripting")]
	[EditorHintImage("Resources", "csharp")]
	public class CSharpScript : ScriptResourceBase
	{
		public new static string FileExt = ".CSharpScript" + Resource.FileExt;

		public CSharpScript()
		{
			
		}

		public CSharpScript(IScriptCompilerService compilerService)
		{
			ScriptCompiler = compilerService;
		}

		protected override void OnLoaded()
		{
			ScriptCompiler = ScriptingPluginCorePlugin.CSharpScriptCompiler;
			base.OnLoaded();
		}
	}
}