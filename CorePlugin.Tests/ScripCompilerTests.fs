namespace CorePlugin.Tests

open NUnit.Framework
open FsCheck
open ScriptingPlugin
open System
open System.Linq
    


module ScriptCompilerTests =

    let myScript = "CSharpScript"
    let scriptPath = "whyIsThisImportant"
    let cShScript = @"namespace Tests
                        {
                            public class MyClass { 
                                public void MyMethod(){var c= 1;}
                            }
                        }"
                        
    let createCSharpCompiler =
        let compiler = new CSharpScriptCompiler()
        let ref = [ "System.dll"; "System.Core.dll"; "Duality.dll" ; "FarseerDuality.dll";"ScriptingPlugin.core.dll"; "OpenTK.dll" ]
        List.map(fun r -> compiler.AddReference(r)) ref |> ignore
        compiler        

    [<Test>]
    let ``Compiler add reference``() = 
        let scriptongCompiler = new CSharpScriptCompiler()
        Check.VerboseThrowOnFailure scriptongCompiler.AddReference               

    [<Test>]
    let ``Compiling doesnt throw on null or empty params``() = 
        let scriptingCompiler = createCSharpCompiler
        Assert.DoesNotThrow( fun () -> scriptingCompiler.Compile(cShScript) |> ignore)
        
    [<Test>]
    let ``Compiling returns true if there is no errors ``() = 
        let scriptingCompiler = createCSharpCompiler
        let compilerResults = scriptingCompiler.Compile(cShScript)
        Assert.IsFalse(compilerResults.Errors.Any())
        Assert.NotNull(compilerResults.CompiledAssembly)


module FsharpScriptCompiler =        
            
    let createFSharpCompiler =
        let compiler = new FSharpScriptCompiler()
        let ref = [ "System.dll"; "System.Core.dll"; "Duality.dll" ; "FarseerDuality.dll";"ScriptingPlugin.core.dll"; "OpenTK.dll";"System.Drawing.dll";"System.Xml.Linq.dll" ]
        List.iter(fun r -> compiler.AddReference(r)) ref
        compiler

    let fsharpScript = @"module Dualityscript

open ScriptingPlugin
open Duality
open System

    type FSharpScript() =
        inherit DualityScript()"

    let join (errors: Collections.Generic.IEnumerable<string>) =     
        String.Join(" ", errors)

    [<Test>]
    let ``Compiler add reference f#``() = 
        let scriptongCompiler = new FSharpScriptCompiler()
        Check.VerboseThrowOnFailure scriptongCompiler.AddReference

    [<Test>]
    let ``Compiling has no errors and creates assembly ``() =        
        let scriptingCompiler = createFSharpCompiler        
        let compiled = scriptingCompiler.Compile(fsharpScript)    

        Assert.IsFalse(compiled.Errors.Any(), join compiled.Errors )
        Assert.NotNull(compiled.CompiledAssembly)
        Assert.IsFalse(String.IsNullOrWhiteSpace compiled.PathToAssembly)

    [<Test>]
    let ``Compiling throws when script is empty string ``() =         
        Assert.Throws<System.ArgumentException>(fun () -> createFSharpCompiler.Compile("") |>ignore )|> ignore