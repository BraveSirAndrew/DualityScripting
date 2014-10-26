using System.Collections.Generic;
using System.IO;
using System.Linq;
using Duality;
using NUnit.Framework;
using ScriptingPlugin;
using ScriptingPlugin.Resources;

namespace CorePlugin.Test.CSharp
{
	[TestFixture]
	public class ScriptComponentTests
	{
		[TestFixture]
		public class TheGetScriptPropertyValuesMethod
		{
			[Test]
			public void ReturnsNullWhenAPropertyDoesntExist()
			{
				var component = new ScriptComponent();
				Assert.IsNull(component.GetScriptPropertyValue("doesntExist"));
			}

			[Test]
			public void ReturnsExistingProperties()
			{
				var value = new object();

				var component = new ScriptComponent();
				component.SetScriptPropertyValue("test", value);

				Assert.AreSame(value, component.GetScriptPropertyValue("test"));
			}
		}

	    [TestFixture]
	    public class ReloadingTheScript
	    {
	        [Test]
	        public void DoesntThrow()
	        {
                var scriptComponent = new ScriptComponent();
	            Assert.DoesNotThrow(() => scriptComponent.OnScriptReloaded(null, null));
	        }
	    }

		[TestFixture]
		public class TheEditorUpdateMethod
		{
			public const string TestScriptWithOneProperty = @"using ScriptingPlugin; 
public class TestScript : DualityScript
	{
		public bool EditorUpdateCalled { get; set; }

		public override void EditorUpdate()
		{
			EditorUpdateCalled = true;
		}
	}";

			public ScriptComponent Component { get; set; }

			[SetUp]
			public void SetUp()
			{
				Component = CreateScriptComponent();
			}

			[Test]
			public void DoesNothingWhenTheScriptInstanceIsNull()
			{
				Assert.DoesNotThrow(() => ((ICmpEditorUpdatable)Component).OnUpdate());
			}

			[Test]
			public void CallsTheScriptsEditorUpdateMethod()
			{
			    if (Directory.Exists("Scripts"))
			    {
			        Directory.Delete("Scripts", true);
			        Directory.CreateDirectory("Scripts");
			    }

				var resource = new TestScriptResource { Script = TestScriptWithOneProperty, SourcePath = "TestScript.cs" };
				resource.Save("TestScript.cs");
				Component.Script = new ContentRef<ScriptResourceBase>(resource);
				Component.SetScriptPropertyValue("EditorUpdateCalled", false);
				Component.OnInit(Duality.Component.InitContext.Activate);

				((ICmpEditorUpdatable) Component).OnUpdate();

				Assert.IsTrue((bool)Component.ScriptPropertyValues["EditorUpdateCalled"]);
			}

			private static ScriptComponent CreateScriptComponent()
			{
				return new ScriptComponent();
			}

			public class TestScriptResource : CSharpScript
			{
				public TestScriptResource()
				{
					var cSharpScriptCompiler = new CSharpScriptCompiler();
					cSharpScriptCompiler.AddReference("Duality.dll");
					cSharpScriptCompiler.AddReference("ScriptingPlugin.core.dll");

					ScriptCompiler = new ScriptCompilerService(cSharpScriptCompiler, null);
				}
			}
		}
	}
}
