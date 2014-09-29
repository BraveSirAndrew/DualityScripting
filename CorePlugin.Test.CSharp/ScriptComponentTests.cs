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
	}
}
