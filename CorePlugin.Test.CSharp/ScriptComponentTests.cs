using System;
using System.Collections;
using System.Collections.Generic;
using Duality;
using Flow;
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
			    var resource = TestScriptFactory.CreateScriptResource(TestScriptWithOneProperty);

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
				var component = new ScriptComponent {Script = TestScriptFactory.CreateScriptResource(TestScriptWithCoroutine)};

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

		[TestFixture]
		public class TheOnShutdownMethod
		{
			public const string TestScriptWithShutdown = @"using ScriptingPlugin; 
using System.Collections.Generic;
using Flow;
using System;

public class TestScript : DualityScript
	{
		public bool ShutdownCalled { get; set; }
		public bool SavingCalled { get; set; }

		public override void Shutdown()
		{
			ShutdownCalled = true;
		}

		public override void Saving()
		{
			SavingCalled = true;
		}
	}";

			[Test]
			[Sequential]
			public void CallsShutdownOrSavingDependingOnContext(
				[Values(Component.ShutdownContext.Saving, Component.ShutdownContext.Deactivate)]Component.ShutdownContext context,
				[Values(false, true)]bool expectedShutdownResult,
				[Values(true, false)]bool expectedSavingResult)
			{
				var component = new ScriptComponent { Script = CreateScriptResource(TestScriptWithShutdown) };

				component.OnInit(Component.InitContext.Activate);
				component.SetScriptPropertyValue("ShutdownCalled", false);
				component.SetScriptPropertyValue("SavingCalled", false);
				component.OnShutdown(context);

				((ICmpEditorUpdatable)component).OnUpdate();

				Assert.AreEqual(expectedShutdownResult, (bool)component.ScriptPropertyValues["ShutdownCalled"]);
				Assert.AreEqual(expectedSavingResult, (bool)component.ScriptPropertyValues["SavingCalled"]);
			}
		}
	}
}
