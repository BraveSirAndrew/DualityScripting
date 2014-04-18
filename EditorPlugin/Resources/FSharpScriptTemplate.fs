module Dualityscript

open ScriptingPlugin
open Duality

    type blaaa() =
        inherit DualityScript()
        
            override this.Update () =
                printfn "updated"