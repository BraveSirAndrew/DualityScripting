using Duality.Editor;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class OpenScriptResourceEditorAction : EditorSingleAction<ScriptResource>
	{
		public override string Name
		{
			get { return "Open script file"; }
		}

		public override string Description
		{
			get { return "Open a script file in the associated editor"; }
		}
		
		public override void Perform(ScriptResource script)
		{
			if (script == null)
				return;
			
			FileImportProvider.OpenSourceFile(script, ".cs", script.SaveScript);
		}

		public override bool MatchesContext(string context)
		{
			return context == DualityEditorApp.ActionContextOpenRes;
		}
	}
}