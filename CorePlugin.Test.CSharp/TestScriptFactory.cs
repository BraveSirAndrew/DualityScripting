using System.IO;
using ScriptingPlugin;
using ScriptingPlugin.Resources;

namespace CorePlugin.Test.CSharp
{
	public static class TestScriptFactory
	{
		public static TestScriptResource CreateScriptResource(string script)
		{
			if (Directory.Exists("Scripts"))
			{
				Directory.Delete("Scripts", true);
				Directory.CreateDirectory("Scripts");
			}

			var resource = new TestScriptResource { Script = script, SourcePath = "TestScript.cs" };
			resource.Save("TestScript.cs");
			return resource;
		}

		public class TestScriptResource : CSharpScript
		{
			public TestScriptResource()
			{
				var cSharpScriptCompiler = new CSharpScriptCompiler();
				cSharpScriptCompiler.AddReference("Duality.dll");
				cSharpScriptCompiler.AddReference("ScriptingPlugin.core.dll");
				cSharpScriptCompiler.AddReference("Flow.dll");

				ScriptCompiler = new ScriptCompilerService(cSharpScriptCompiler, null);
			}
		}
	}
}