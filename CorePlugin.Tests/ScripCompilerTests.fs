﻿namespace CorePlugin.Tests

open NUnit.Framework
open FsCheck
open ScriptingPlugin
open System
open System.Linq
open System.Reflection    

module ScriptCompilerTests =

    let myScript = "CSharpScript"
    let scriptPath = "whyIsThisImportant"
    let cShScript = @"namespace Tests
                        {
                            public class MyClass { 
                                public void MyMethod(){var c= 1;}
                            }
                        }"
    let errorCSharpScript = @"namespace Tests
                        {
                            public class MyClass { 
                                public void MyMethod(){
                                c;
                                }
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
    let ``Compiling doesnt throw when SourcePath is null``() = 
        let scriptingCompiler = createCSharpCompiler
        let newthing= new CompilationUnit(cShScript, null)
        Assert.DoesNotThrow( fun () -> scriptingCompiler.Compile([newthing]) |> ignore)

    [<Test>]
    let ``Compiling with destination assembly``() = 
        
        let loc =  "Scripts"
        
        let scriptingCompiler = createCSharpCompiler
        let newthing= new CompilationUnit(cShScript, null)
        Assert.DoesNotThrow( fun () -> scriptingCompiler.Compile([newthing], loc) |> ignore)

    [<Test>]
    let ``Compiling doesnt throw when SourcePath is not a valid path``() = 
        let scriptingCompiler = createCSharpCompiler
        Assert.DoesNotThrow( fun () -> scriptingCompiler.Compile(cShScript, "not a path") |> ignore)

    [<Test>]
    let ``Compiling doesnt throw on null or empty params``() = 
        let scriptingCompiler = createCSharpCompiler
        Assert.DoesNotThrow( fun () -> scriptingCompiler.Compile(cShScript) |> ignore)
        
    [<Test>]
    let ``Compiling returns true if there are no errors ``() = 
        let scriptingCompiler = createCSharpCompiler
        let compilerResults = scriptingCompiler.Compile(cShScript)
        Assert.IsFalse(compilerResults.Errors.Any())
        Assert.NotNull(compilerResults.CompiledAssembly)

    [<Test>]
    let ``Compiling returns false if there are  errors ``() = 
        let scriptingCompiler = createCSharpCompiler
        let compilerResults = scriptingCompiler.Compile(errorCSharpScript)

        Assert.IsTrue(compilerResults.Errors.Any(), String.Join("\n ", compilerResults.Errors))
        Assert.AreEqual(2,compilerResults.Errors.Count())
        
    [<Test>]
    let ``Compiling throws when script is empty string ``() =                 
        Assert.Throws<System.ArgumentException>(fun () -> createCSharpCompiler.Compile("") |> ignore )|> ignore

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
        Assert.Throws<System.ArgumentException>(fun () -> createFSharpCompiler.Compile("") |> ignore )|> ignore

    [<Test>]
    let ``Compiling with destination assembly``() =         
        let loc =  "Scripts"        
        let scriptingCompiler = createFSharpCompiler
        let newthing= new CompilationUnit(fsharpScript, null)
        Assert.DoesNotThrow( fun () -> scriptingCompiler.Compile([newthing], loc) |> ignore)

    [<Test>]
    let ``Compiling multiple scripts``() =         
        let loc =  "Scripts"        
        let f1 = @"module Bla

    let longBeaked = ""Delphinus capensis""
    let shortBeaked = ""Delphinus delphis""
    let dolphins = [longBeaked; shortBeaked]
    printfn ""Known Dolphins: %A"" dolphins"
        let f2 = @"module Whales.Fictional
/// The three kinds of whales we cover in this release
type WhaleKind =
| Blue
| Killer
| GreatWhale
/// The main whale
let moby = ""Moby Dick Pacific"", GreatWhale
/// The backup whale
let bluey = ""Blue, Southern Ocean"", Blue
/// This whale is for experimental use only
let orca = ""Orca, Pacific"", Killer
/// The collected whales
let whales = [|moby; bluey; orca|]"

        let scriptingCompiler = createFSharpCompiler
        let newthing= [new CompilationUnit(fsharpScript, null); new CompilationUnit(f1);new CompilationUnit(f2)]
        Assert.DoesNotThrow( fun () -> scriptingCompiler.Compile(newthing, loc) |> ignore)

