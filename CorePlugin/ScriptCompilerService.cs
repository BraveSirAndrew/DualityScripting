using System;
using System.Linq;
using Duality;

namespace ScriptingPlugin
{
	public class ScriptCompilerService : IScriptCompilerService
	{
		private readonly IScriptCompiler _scriptCompiler;
		private IPdbEditor _pdbEditor;

		public ScriptCompilerService(IScriptCompiler scriptCompiler)
		{
			_scriptCompiler = scriptCompiler;
		}

		public ScriptCompilerResult TryCompile(string scriptName, string scriptPath, string script)
		{
			try
			{
				Log.Editor.Write("Compiling script '{0}'.", scriptName);
				var compilerResult = _scriptCompiler.Compile(script, scriptPath);

				if (compilerResult.Errors.Any() == false)
				{
					if (_pdbEditor != null)
						_pdbEditor.SetSourcePathInPdbFile(compilerResult.PathToAssembly, scriptName, scriptPath);
				}

				if (compilerResult.Errors.Any())
				{
					var text = string.Join(Environment.NewLine, compilerResult.Errors);
					Log.Editor.WriteError("Error with script '{0}': {1}", scriptName, Escape(text));
					return new ScriptCompilerResult(CompilerResult.CompilerError);
				}

				return new ScriptCompilerResult(CompilerResult.AssemblyExists, compilerResult.CompiledAssembly);
			}
			catch (Exception exception)
			{
				Log.Editor.WriteError("Could not compile script {0} error {1}", scriptName, exception);
				return new ScriptCompilerResult(CompilerResult.GeneralError);
			}
		}

		public void SetPdbEditor(IPdbEditor pdbEditor)
		{
			_pdbEditor = pdbEditor;
		}

		private static string Escape(string text)
		{
			return text.Replace("{", "{{").Replace("}", "}}");
		}
	}
}