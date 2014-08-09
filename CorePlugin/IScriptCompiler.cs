﻿using System.Collections.Generic;

namespace ScriptingPlugin
{
	public interface IScriptCompiler
	{
		IScriptCompilerResults Compile(string script, string sourceFilePath = null);
		IScriptCompilerResults Compile(IEnumerable<CompilationUnit> scripts, string resultingAssemblyDirectory = null);
	}
}