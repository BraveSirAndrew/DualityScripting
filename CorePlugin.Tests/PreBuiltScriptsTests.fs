namespace CorePlugin.Tests

module ``Prebuilt scripts Tests`` =

    open NUnit.Framework    
    open ScriptingPlugin
    open System.Reflection    
    open System.IO

    let createFSharpCompiler =
        let compiler = new FSharpScriptCompiler()
        let ref = [ "System.dll"; "System.Core.dll"; "Duality.dll" ; "FarseerDuality.dll";"ScriptingPlugin.core.dll"; "OpenTK.dll";"System.Drawing.dll";"System.Xml.Linq.dll" ]
        List.iter(fun r -> compiler.AddReference(r)) ref
        compiler

    let createDirWithAssemblies() =
        let fsharpScript:string = @"module Dualityscript

open ScriptingPlugin
open Duality
open System

    type FSharpScript() =
        inherit DualityScript()"
        
        if Directory.Exists "Scripts" then             
            Directory.Delete ("Scripts", true)
        Directory.CreateDirectory "Scripts" |> ignore
        let compiler = createFSharpCompiler        
        let results = compiler.Compile(fsharpScript)        
        File.Copy(results.PathToAssembly, Path.Combine("Scripts", Path.GetFileName(results.PathToAssembly)))
(*
    [<Test>]
    let ``When scripts dir doesn't exist then return empty`` ()=
        if Directory.Exists "Scripts" then Directory.Delete ("Scripts", true)
        let empty: Assembly [] = [||]
        let pre = PrebuildScripts.LoadAssemblies() 
        Assert.AreEqual (empty.Length, pre.Length)

    [<Test>]
    let ``When scripts dir exist but empty then return empty`` ()=
        if not (Directory.Exists "Scripts") then Directory.CreateDirectory "Scripts" |> ignore        
        let pre = PrebuildScripts.LoadAssemblies() 
        Assert.IsEmpty pre

    [<Test>]
    let ``When there is assemblies then not empty ``() =        
        createDirWithAssemblies()
        let pre = PrebuildScripts.LoadAssemblies() 
        Assert.IsNotEmpty pre
*)

    [<Test>]
    let ``When there is assemblies no need to load twice ``() =        
        createDirWithAssemblies()
        let stopwatch = System.Diagnostics.Stopwatch()
        stopwatch.Start() 

        stopwatch.Restart()
        let fst = PrebuildScripts.LoadAssemblies()
        stopwatch.Stop()
        let first = stopwatch.ElapsedMilliseconds
        
        stopwatch.Restart()        
        let scn = PrebuildScripts.LoadAssemblies()
        stopwatch.Stop()
        let ult = stopwatch.ElapsedMilliseconds

        Assert.Greater (first, ult, "ult {0} first {1}", ult, first)
        Assert.AreEqual (fst.Length, scn.Length)
        
        
