using Duality;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public interface IResourceSaver
	{
		ResourceSaver.ResourceData Save(ResourceEventArgs resourceEventArgs);
	}
	public class ResourceSaver : IResourceSaver
	{
		private readonly ProjectConstants _projectConstants;

		public ResourceSaver(ProjectConstants projectConstants)
		{
			_projectConstants = projectConstants;
		}
		public struct ResourceData
		{
			public ResourceData(string fullName, string extension, string projectPath) : this()
			{
				ScriptFullName = fullName;
				ScriptExtension = extension;
				ProjectPath = projectPath;
			}

			public string ProjectPath { get; private set; }
			public string ScriptFullName { get;  private set; }
			public string ScriptExtension { get; private set; }	 
		}
		
		public ResourceData Save(ResourceEventArgs resourceEventArgs)
		{
			string scriptFullName = null;
			string scriptExtension = null;
			string projectPath = null;
			if (resourceEventArgs.ContentType == typeof(CSharpScript))
			{
				scriptFullName = SaveResource<CSharpScript>(resourceEventArgs.Content, Resources.Resources.ScriptTemplate);
				scriptExtension = _projectConstants.CSharpScriptExtension;
				projectPath = _projectConstants.CSharpProjectPath;
			}
			else if (resourceEventArgs.ContentType == typeof(FSharpScript))
			{
				scriptFullName = SaveResource<FSharpScript>(resourceEventArgs.Content, Resources.Resources.FSharpScriptTemplate);
				scriptExtension = _projectConstants.FSharpScriptExtension;
				projectPath = _projectConstants.FSharpProjectPath;

			}
			return new ResourceData(scriptFullName,scriptExtension, projectPath);
		}
		private string SaveResource<T>(ContentRef<Resource> scriptResourceContentRef, string script) where T : ScriptResourceBase
		{
			var scriptResource = scriptResourceContentRef.As<T>();

			scriptResource.Res.Script = script;
			scriptResource.Res.Save();
			return scriptResource.FullName;
		}
	}
}