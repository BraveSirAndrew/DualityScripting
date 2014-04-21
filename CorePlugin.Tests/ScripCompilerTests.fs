namespace CorePlugin.Tests

open NUnit.Framework
open FsCheck
open ScriptingPlugin

module ScriptCompilerTests =

    type blaaa() =
        inherit DualityScript()
        
            override this.Update () =
                printfn "updated"

    [<Test>]
    let ``Compiler add reference "I"s``() = 
        let scriptongCompiler = new CSharpScriptCompiler()
        Check.Verbose scriptongCompiler.AddReference        

    [<Test>]
    let ``Compiler add reference "f"s``() = 
        let scriptongCompiler = new FSharpScriptCompiler()
        Check.Verbose scriptongCompiler.AddReference        

    [<Test>]
    let ``Test DualityScript Update method``() =         
        Assert.DoesNotThrow(fun () -> blaaa().Update())        