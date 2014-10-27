using System.IO;

namespace ScriptingPlugin
{
    public class PathProvider
    {
        public static string GetAssemblyPath(string tempFileName, string nonDefault = null)
        {
            var assemblyDirectory = GetAssemblyDirectory(nonDefault);

            var assemblyName = tempFileName + ".dll";
            return Path.Combine(assemblyDirectory, assemblyName);
        }

        public static string GetAssemblyDirectory(string nonDefault)
        {
            var assemblyDirectory = string.IsNullOrWhiteSpace(nonDefault)
                ? FileConstants.AssembliesDirectory
                : nonDefault;
            if (!Directory.Exists(assemblyDirectory))
                Directory.CreateDirectory(assemblyDirectory);
            return assemblyDirectory;
        }

        public static void CreateAssembliesDirectory()
        {
            if (Directory.Exists(FileConstants.AssembliesDirectory) == false)
                Directory.CreateDirectory(FileConstants.AssembliesDirectory);
        }
    }
}