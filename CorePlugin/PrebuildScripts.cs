using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Duality;

namespace ScriptingPlugin
{
	public class PrebuildScripts
	{
		private static List<Assembly> _resultingAssemblies;

		public static Assembly[] LoadAssemblies()
		{
			var scriptsDirectory = new DirectoryInfo("Scripts");
			if (!scriptsDirectory.Exists)
				return new Assembly[]{};
			
			_resultingAssemblies = _resultingAssemblies ?? new List<Assembly>();
			if( _resultingAssemblies.Count == 0)
			{
				var scriptsDll = scriptsDirectory.GetFiles("*.dll", SearchOption.TopDirectoryOnly);
				foreach (var scriptAssembly in scriptsDll)
				{
					if (!scriptAssembly.Exists)
						continue;
					var assembly = Assembly.LoadFile(Path.GetFullPath(scriptAssembly.FullName));
					_resultingAssemblies.Add(assembly);
					Log.Editor.Write("Loading script assembly {0} from Scripts directory", assembly.FullName);
				}
			}
			return _resultingAssemblies.ToArray();
		}
	}
}