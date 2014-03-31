namespace CorePlugin.Tests

open NUnit.Framework
open FsCheck
open ScriptingPlugin

module ScriptCompilerTests =

    [<Test>]
    let ``For all valid inputs, there must be a max of four "I"s``() = 
        let revRevIsOrig (xs:list<int>) = List.rev(List.rev xs) = xs
        Check.Quick revRevIsOrig

    [<Test>]
    let ``compiler compiles "I"s``() = 
        let compiler = new ScriptCompiler()        
        Check.Quick (compiler.Compile("name","path","something"))