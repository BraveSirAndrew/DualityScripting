using NUnit.Framework;
using ScriptingPlugin;

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
	}
}
