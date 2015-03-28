namespace Dualityscript

open ScriptingPlugin
open Duality

type FSharpScript() =
    inherit DualityScript()
    
        override this.Update () =
            Log.Editor.Write "updated"