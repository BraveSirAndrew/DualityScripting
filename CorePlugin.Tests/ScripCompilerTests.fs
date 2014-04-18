namespace CorePlugin.Tests

open NUnit.Framework
open FsCheck
open ScriptingPlugin

module ScriptCompilerTests =

    [<Test>]
    let ``Compiler add reference "I"s``() = 
        let scriptongCompiler = new CSharpScriptCompiler()
        Check.Verbose scriptongCompiler.AddReference        

    [<Test>]
    let ``Compiler add reference "f"s``() = 
        let scriptongCompiler = new FSharpScriptCompiler()
        Check.Verbose scriptongCompiler.AddReference        