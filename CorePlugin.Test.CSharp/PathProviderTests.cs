using System.IO;
using NUnit.Framework;
using ScriptingPlugin;

namespace CorePlugin.Test.CSharp
{
    [TestFixture]
    public class PathProviderTests
    {
        [Test]
        public void When_default_path_Then_assemblies()
        {
            var result = PathProvider.GetAssemblyDirectory(null);
            Assert.AreEqual(FileConstants.AssembliesDirectory, result);
        }

        [Test]
        public void When_non_default_path_Then_use_non_default()
        {
            var nondefault = "nondef";
            var result = PathProvider.GetAssemblyDirectory(nondefault);
            Assert.AreEqual(nondefault, result);
        }

        [Test]
        public void When_assemblies_directory_doesnt_exist_Then_provider_will_create_it()
        {
            if(Directory.Exists(FileConstants.AssembliesDirectory))
                Directory.Delete(FileConstants.AssembliesDirectory, true);
            Assert.IsFalse(Directory.Exists(FileConstants.AssembliesDirectory));

            var result = PathProvider.GetAssemblyDirectory(null);
            Assert.IsTrue(Directory.Exists(FileConstants.AssembliesDirectory));
        }


    }
}