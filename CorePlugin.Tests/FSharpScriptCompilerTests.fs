namespace CorePlugin.Tests

module ``F# ScriptCompiler tests``=        
    
    open NUnit.Framework
    open FsCheck
    open ScriptingPlugin
    open System
    open System.Linq

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

    [<Ignore "Need to find out how to actually use generators">]
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
    let ``Doesn't add reference to system runtime``() =         
        let scriptingCompiler = createFSharpCompiler
        let ref = "System.Runtime.dll"
        scriptingCompiler.AddReference ref
        Assert.False (scriptingCompiler.References |> Seq.exists (fun x-> x = ref) )


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