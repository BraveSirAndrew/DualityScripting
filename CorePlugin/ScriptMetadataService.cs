using System;
using System.IO;
using System.IO.Abstractions;

namespace ScriptingPlugin
{
	public class ScriptMetadataService : IScriptMetadataService
	{
		private readonly IFileSystem _fileSystem;

		public ScriptMetadataService(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public void UpdateMetadata(string scriptPath)
		{
			var metafilePath = _fileSystem.Path.GetFullPath(GetMetafilePath(scriptPath));

			if (_fileSystem.File.Exists(metafilePath))
			{
				var fileInfo = new FileInfo(metafilePath);
				fileInfo.Attributes &= ~FileAttributes.Hidden;
			}

			_fileSystem.File.WriteAllText(metafilePath, "");
			_fileSystem.File.SetLastWriteTime(metafilePath, DateTime.Now);
			_fileSystem.File.SetAttributes(metafilePath, FileAttributes.Hidden);
		}

		public string GetMetafilePath(string scriptPath)
		{
			return _fileSystem.Path.Combine(_fileSystem.Path.GetDirectoryName(scriptPath), _fileSystem.Path.GetFileNameWithoutExtension(scriptPath) + ".meta");
		}
	}
}