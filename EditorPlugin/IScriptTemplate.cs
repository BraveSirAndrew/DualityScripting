using Duality;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public interface IScriptTemplate
	{
		string Script { get; }
		string FileExtension { get; }
		string ProjectPath { get; }

		void Apply(ContentRef<ScriptResourceBase> scriptResource);
	}
}