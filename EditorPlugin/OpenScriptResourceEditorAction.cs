using Duality.Editor;
using ScriptingPlugin.CSharp;
using ScriptingPlugin.FSharp;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public abstract class OpenScriptResourceEditorAction<TScriptType> : EditorSingleAction<TScriptType> where TScriptType : ScriptResourceBase
	{
		public override string Name
		{
			get { return "Open script file"; }
		}

		public override string Description
		{
			get { return "Open a script file in the associated editor"; }
		}

		
		public override void Perform(TScriptType script)
		{
			if (script == null)
				return;

			FileImportProvider.OpenSourceFile(script, CurrentExtension(), script.SaveScript);
		}

		protected abstract string CurrentExtension();
		

		public override bool MatchesContext(string context)
		{
			return context == DualityEditorApp.ActionContextOpenRes;
		}
	}

	public class OpenCSharpScriptResourceEditorAction : OpenScriptResourceEditorAction<CSharpScript>
	{
		protected override string CurrentExtension()
		{
			return FileConstants.CSharpExtension;
		}
	}

	public class OpenFSharpScriptResourceEditorAction : OpenScriptResourceEditorAction<FSharpScript>
	{
		protected override string CurrentExtension()
		{
			return FileConstants.FSharpExtension;

		}
	}
}