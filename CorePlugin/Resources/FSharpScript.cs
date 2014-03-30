using System;
using Duality;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	public class FSharpScript : ScriptResourceBase
	{
		public new const string FileExt = ".fs" + Resource.FileExt;

		protected override void OnLoaded()
		{
			ScriptCompiler = ScriptingPluginCorePlugin.FSharpScriptCompiler;
			base.OnLoaded();
		}
	}
}