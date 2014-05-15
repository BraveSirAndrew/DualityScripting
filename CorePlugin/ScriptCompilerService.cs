using System;
using System.CodeDom.Compiler;
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

		public CompilerResult TryCompile(string scriptName, string scriptPath, string script, out Assembly assembly)
		{
			assembly = null;
			try
			{
				var compilerResult = _scriptCompiler.Compile(script);

				if (compilerResult.Errors.HasErrors == false)
				{
					if (_pdbEditor != null)
						_pdbEditor.SetSourcePathInPdbFile(compilerResult.PathToAssembly, scriptName, scriptPath);
				}

				if (compilerResult.Errors.HasErrors)
				{
					var text = compilerResult.Errors.Cast<CompilerError>().Aggregate("", (current, compilerError) => current + (Environment.NewLine + compilerError));
					Log.Editor.WriteError("Error compiling script '{0}': {1}", scriptName, text);
					return CompilerResult.CompilerError;
				}
				assembly = compilerResult.CompiledAssembly;
				return CompilerResult.AssemblyExists;
			}
			catch (Exception exception)
			{
				Log.Editor.WriteError("Could not compile script {0} error {1}", scriptName, exception);
				return CompilerResult.GeneralError;
			}
		}

		public void SetPdbEditor(IPdbEditor pdbEditor)
		{
			_pdbEditor = pdbEditor;
		}
	}
}