module Dualityscript

open ScriptingPlugin
open Duality

    type FSharpScript() =
        inherit DualityScript()
        
            override this.Update () =
                printfn "updated"