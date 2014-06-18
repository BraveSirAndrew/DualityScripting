using Duality;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class FSharpScriptTemplate : IScriptTemplate
	{
		public string Script { get { return Resources.Resources.FSharpScriptTemplate; } }
		public string FileExtension { get { return ".fs"; } }
		public string ProjectPath { get; set; }

		public void Apply(ContentRef<ScriptResourceBase> scriptResource)
		{
			scriptResource.Res.Script = Script;
			scriptResource.Res.Save();
		}
	}
}