using Duality;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class CSharpScriptTemplate : IScriptTemplate
	{
		public string Script { get { return Resources.Resources.CSharpScriptTemplate; } }
		public string FileExtension { get { return ".cs"; } }
		public string ProjectPath { get; set; }

		public void Apply(ContentRef<ScriptResourceBase> scriptResource)
		{
			scriptResource.Res.Script = Script;
			scriptResource.Res.Save();
		}
	}
}