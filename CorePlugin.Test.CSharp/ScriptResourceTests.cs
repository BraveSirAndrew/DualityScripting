using NUnit.Framework;

namespace CorePlugin.Test.CSharp
{
	[TestFixture]
	public class ScriptResourceTests
	{
		[TestFixture]
		public class TheInstantiateMethod
		{
			public const string TestScriptWithDeepInheritance = @"using ScriptingPlugin; 
using System;

public class BaseClass : DualityScript
{
}

public class TestScript : BaseClass
	{
		
	}";

			public const string TestScriptSimple = @"using ScriptingPlugin; 
using System;

public class TestScript : DualityScript
	{
		
	}";

			[Test]
			public void FindsTheTypeWhenItInheritsFromDualityScript()
			{
				var script = TestScriptFactory.CreateScriptResource(TestScriptSimple);

				var instance = script.Instantiate();

				Assert.NotNull(instance);
			}

			[Test]
			public void FindsTheTypeWhenItDoesntDirectlyInheritFromDualityScript()
			{
				var script = TestScriptFactory.CreateScriptResource(TestScriptWithDeepInheritance);
				
				var instance = script.Instantiate();

				Assert.NotNull(instance);
			}
		}
	}
}