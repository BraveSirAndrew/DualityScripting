namespace CorePlugin.Tests

open NUnit.Framework
open FsCheck
open ScriptingPlugin

module ScriptCompilerTests =

    let myScript = "CSharpScript"
    let scriptPath = "whyIsThisImportant"
    let cShScript = @"namespace Tests
                        {
                            public class MyClass { 
                                public void MyMethod(){var c= 1;}
                            }
                        }"
    
    [<Test>]
    let ``Compiler add reference "I"s``() = 
        let scriptongCompiler = new CSharpScriptCompiler()
        Check.VerboseThrowOnFailure scriptongCompiler.AddReference               

    [<Test>]
    let ``Compiling doesnt throw on null or empty params``() = 
        let scriptingCompiler = new CSharpScriptCompiler()
        Assert.DoesNotThrow( fun () -> scriptingCompiler.TryCompile("", null, "some") |> ignore)
        
    [<Test>]
    let ``Compiling returns null on null or empty params``() = 
        let scriptingCompiler = new CSharpScriptCompiler()              
        let compiled = scriptingCompiler.TryCompile(null, null, null)
        Assert.AreEqual(CompilerResult.GeneralError,fst compiled)

    [<Test>]
    let ``Compiling returns true if there is no errors ``() = 
        let scriptingCompiler = new CSharpScriptCompiler()
        let compiled = scriptingCompiler.TryCompile(myScript, scriptPath, cShScript)
        Assert.AreEqual(CompilerResult.AssemblyExists, fst compiled)         
        Assert.NotNull(snd compiled) 

module FsharpScriptCompiler =
        
    [<Test>]
    let ``Compiler add reference "f"s``() = 
        let scriptongCompiler = new FSharpScriptCompiler()
        Check.VerboseThrowOnFailure scriptongCompiler.AddReference   