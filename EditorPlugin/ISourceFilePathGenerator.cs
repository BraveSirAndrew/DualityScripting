using Duality;

namespace ScriptingPlugin.Editor
{
	public interface ISourceFilePathGenerator
	{
		string GenerateSourceFilePath(ContentRef<Resource> resource, string fileExtension);
	}
}