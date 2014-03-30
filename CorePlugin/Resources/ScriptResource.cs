using System;
using Duality;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	public class ScriptResource : ScriptResourceBase
	{
		public new const string FileExt = ".cs" + Resource.FileExt;

		protected override void OnLoaded()
		{
			ScriptCompiler = ScriptingPluginCorePlugin.CSharpScriptCompiler;
			base.OnLoaded();
		}
	}
}
