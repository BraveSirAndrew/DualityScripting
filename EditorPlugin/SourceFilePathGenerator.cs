using Duality;
using Duality.Editor;

namespace ScriptingPlugin.Editor
{
	public class SourceFilePathGenerator : ISourceFilePathGenerator
	{
		public string GenerateSourceFilePath(ContentRef<Resource> resource, string fileExtension)
		{
			return FileImportProvider.GenerateSourceFilePath(resource, fileExtension);
		}
	}
}