namespace CorePlugin.Tests

module ``C# ScriptCompiler Tests`` =

    open NUnit.Framework
    open FsCheck
    open ScriptingPlugin
    open System
    open System.Linq

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
        let comp = new CSharpScriptCompiler()
        let reference = FsCheck.Gen.elements ["System";"System.Xml.Linq"]
        let addrefParams = Arb.fromGen reference
        
        Check.Quick (Prop.forAll (addrefParams) comp.AddReference)

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

    let cShScriptNested = @"namespace Tests
                    {
                        public class MyClass { 
                            public void MyMethod(){var c= 1;}

                            public class MyInternalClass {}

                            public void TestMethod()
                            {
                                var a = new MyInternalClass();
                            }
                        }
                    }"
    [<Test>]
    let ``Can compile internal classes ``() = 
        let scriptingCompiler = createCSharpCompiler
        let compilerResults = scriptingCompiler.Compile(cShScriptNested)
        Assert.IsFalse(compilerResults.Errors.Any())
        Assert.NotNull(compilerResults.CompiledAssembly)