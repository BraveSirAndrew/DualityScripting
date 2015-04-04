using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Duality;
using Flow;
using NUnit.Framework;
using ScriptingPlugin;
using ScriptingPlugin.CSharp;
using ScriptingPlugin.Resources;

namespace CorePlugin.Test.CSharp
{
	[TestFixture]
	public class ScriptComponentTests
	{
		protected static TestScriptResource CreateScriptResource(string script)
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

				ScriptCompiler = new ScriptCompilerService(cSharpScriptCompiler);
			}
		}

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
			    var resource = CreateScriptResource(TestScriptWithOneProperty);

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
		}

		[TestFixture]
		public class TheStartCoroutineMethod
		{
			public const string TestScriptWithCoroutine = @"using ScriptingPlugin; 
using System.Collections.Generic;
using Flow;
using System;

public class TestScript : DualityScript
	{
		public IEnumerator<object> Coroutine(IGenerator t0)
		{
			Console.WriteLine(""Test"");
			yield return 1;
			yield return 2;
			yield return 3;
		}
	}";

			[Test]
			public void ReturnsAValidCoroutineInstance()
			{
				var component = new ScriptComponent {Script = CreateScriptResource(TestScriptWithCoroutine)};

				component.OnInit(Component.InitContext.Activate);
				var factory = Create.NewFactory();
				var coroutine = component.StartCoroutine<object>("Coroutine", factory);

				Assert.IsNotNull(coroutine);

				coroutine.Step();
				factory.Kernel.Step();
				Assert.AreEqual(1, coroutine.Value);

				factory.Kernel.Step();
				Assert.AreEqual(2, coroutine.Value);

				factory.Kernel.Step();
				Assert.AreEqual(3, coroutine.Value);
			}
		}
	}
}
