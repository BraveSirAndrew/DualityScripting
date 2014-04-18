using System;
using Duality;

namespace ScriptingPlugin.Resources
{
	[Serializable]
	public class ScriptResource : ScriptResourceBase
	{
		public new const string FileExt = FileConstants.CSharpExtension + Resource.FileExt;

		protected override void OnLoaded()
		{
			ScriptCompiler = ScriptingPluginCorePlugin.CSharpScriptCompiler;
			base.OnLoaded();
		}
	}
}
