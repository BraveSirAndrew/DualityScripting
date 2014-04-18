namespace CorePlugin.Tests

open NUnit.Framework
open FsCheck
open ScriptingPlugin

module ScriptCompilerTests =

    [<Test>]
    let ``Compiler add reference "I"s``() = 
        let scriptongCompiler = new ScriptCompiler()
        Check.Verbose scriptongCompiler.AddReference        