using System;
using System.Linq;
using System.Reflection;
using Duality;

namespace ScriptingPlugin
{
	public class ScriptCompilerService : IScriptCompilerService
	{
		private readonly IScriptCompiler _scriptCompiler;
		private IPdbEditor _pdbEditor;

		public ScriptCompilerService(IScriptCompiler scriptCompiler, IPdbEditor pdbEditor)
		{
			_scriptCompiler = scriptCompiler;
			_pdbEditor = pdbEditor;
		}

		public ScriptsResult TryCompile(string scriptName, string scriptPath, string script, out Assembly assembly)
		{
			assembly = null;
			try
			{
				var compilerResult = _scriptCompiler.Compile(script);

				if (compilerResult.Errors.Any() == false)
				{
					if (_pdbEditor != null)
						_pdbEditor.SetSourcePathInPdbFile(compilerResult.PathToAssembly, scriptName, scriptPath);
				}

				if (compilerResult.Errors.Any())
				{
					var text = string.Join(Environment.NewLine, compilerResult.Errors);
					Log.Editor.WriteError("Error with script '{0}': {1}", scriptName, text);
					return ScriptsResult.CompilerError;
				}
				assembly = compilerResult.CompiledAssembly;
				return ScriptsResult.AssemblyExists;
			}
			catch (Exception exception)
			{
				Log.Editor.WriteError("Could not compile script {0} error {1}", scriptName, exception);
				return ScriptsResult.GeneralError;
			}
		}

		public void SetPdbEditor(IPdbEditor pdbEditor)
		{
			_pdbEditor = pdbEditor;
		}
	}
}