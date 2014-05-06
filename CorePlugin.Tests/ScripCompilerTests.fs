namespace CorePlugin.Tests

open NUnit.Framework
open FsCheck
open ScriptingPlugin

module TestHelpers =
    
    let getLogInfo ()=  
                Duality.Log.LogData.Data
                            |> Seq.map (fun x-> x.Message)
                            |> String.concat "+"

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
        Assert.DoesNotThrow( fun () -> scriptingCompiler.TryCompile("", null, "soe") |> ignore)
        
    [<Test>]
    let ``Compiling returns null on null or empty params``() = 
        let scriptingCompiler = createCSharpCompiler             
        let compiled = scriptingCompiler.TryCompile(null, null, null)
        Assert.AreEqual(CompilerResult.GeneralError,fst compiled)

    [<Test>]
    let ``Compiling returns true if there is no errors ``() = 
        let scriptingCompiler = createCSharpCompiler
        let compiled = scriptingCompiler.TryCompile(myScript, scriptPath, cShScript)
        Assert.AreEqual(CompilerResult.AssemblyExists, fst compiled)
        Assert.NotNull(snd compiled) 

module FsharpScriptCompiler =        
            
    let createFSharpCompiler =
        let compiler = new FSharpScriptCompiler()
        let ref = [ "System.dll"; "System.Core.dll"; "Duality.dll" ; "FarseerDuality.dll";"ScriptingPlugin.core.dll"; "OpenTK.dll" ]
        List.map(fun r -> compiler.AddReference(r)) ref |> ignore
        compiler
    
    [<Test>]
    let ``Compiler add reference f#``() = 
        let scriptongCompiler = new FSharpScriptCompiler()
        Check.VerboseThrowOnFailure scriptongCompiler.AddReference

    [<Test>]
    let ``Compiling returns true if there is no errors ``() = 
        let scriptingCompiler = createFSharpCompiler
        //no need for the api to use script name and the rest, just the script would work, tho the path is better
        let compiled = scriptingCompiler.TryCompile("asdas", "get a real path", "asd")
        //why this gets the value at the start
        let error = lazy TestHelpers.getLogInfo

//        Assert.AreEqual(CompilerResult.AssemblyExists, fst compiled )
//        Assert.NotNull(snd compiled) 
        Assert.Ignore("Ignore this integration test for now")



// Other notes
// need to support rename of the script
// replace template file
// copile all scripts into a dll  add to fsproj and work also on edit