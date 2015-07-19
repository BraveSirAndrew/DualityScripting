// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile

// Directories
let buildDir  = @".\build\"
let testDir   = @".\test\"
let packagesDir = @".\deploy\"
// version info
let version = if isLocalBuild then "0.3-local" else "0.3."+buildVersion



type ProjectInfo = { 
    
    Authors: string list
    Description: string
    Version: string
  }
let info = {  
  Authors= ["Andrea Magnorsky";"Andrew O'Connor"; ]
  Description =  "Android runtime for DualityScripting plugin";
  Version = if isLocalBuild then "0.2-local" else "0.2."+buildVersion
}

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; packagesDir] 
)

Target "SetVersions" (fun _ ->
    CreateCSharpAssemblyInfo "./CorePlugin/Properties/AssemblyInfo.cs"
        [Attribute.Title "Duality Scripting"
         Attribute.Description "Scripting for Duality"         
         Attribute.Product "DualityScripting"
         Attribute.Version version
         Attribute.FileVersion version]
    CreateCSharpAssemblyInfo "./EditorPlugin/Properties/AssemblyInfo.cs"
        [Attribute.Title "Duality Scripting Editor"
         Attribute.Description "Scripting for Duality. Editing assemblies"         
         Attribute.Product "DualityScriptingEditor"
         Attribute.Version version
         Attribute.FileVersion version]
    CreateCSharpAssemblyInfo "./ScriptingCSCorePlugin/Properties/AssemblyInfo.cs"
        [Attribute.Title "Duality Scripting C#"
         Attribute.Description "Scripting for Duality C# dependencies"         
         Attribute.Product "DualityScriptingCSharp"
         Attribute.Version version
         Attribute.FileVersion version]
    CreateCSharpAssemblyInfo "./ScriptingFSCorePlugin/Properties/AssemblyInfo.cs"
        [Attribute.Title "Duality Scripting F#"
         Attribute.Description "Scripting for Duality f# dependencies"         
         Attribute.Product "DualityScriptingFSharp"
         Attribute.Version version
         Attribute.FileVersion version]
    CreateCSharpAssemblyInfo "./ScriptingPlugin.Android/Properties/AssemblyInfo.cs"
        [Attribute.Title "Duality Scripting Android runtime"
         Attribute.Description "Android runtime for Scripting on Duality dependencies"         
         Attribute.Product "DualityScriptingAndroid"
         Attribute.Version version
         Attribute.FileVersion version]
)

Target "RestorePackages" (fun _ ->
    !! "./**/packages.config"
        |> Seq.iter (RestorePackage (fun p ->
            { p with Sources = ["https://www.myget.org/F/6416d9912a7c4d46bc983870fb440d25/"]}))
)
let buildMode = getBuildParamOrDefault "buildMode" "Release"

Target "Build" (fun _ ->              
    let setParams defaults =
        { defaults with
            Verbosity = Some(Normal)            
            Properties = ["Configuration", buildMode ]
        }
    build setParams "./DualityScriptingPlugins.sln"    
    |> DoNothing  
)

Target "BuildAndroid" (fun _ ->              
    let setParams defaults =
        { defaults with
            Verbosity = Some(Normal)            
            Properties = ["Configuration", buildMode ]
        }
    build setParams "./DualityScriptingPlugins.Android.sln"
    |> DoNothing  
)

Target "BuildTest" (fun _ ->
    !! "**/*.Test*.*sproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "NUnitTest" (fun _ ->         
     !! (testDir + @"\*Test*.*.dll") 
      |> NUnit (fun p ->
                 {p with
                   DisableShadowCopy = true;
                   OutputFile = testDir + @"TestResults.xml"})
)

Target "CreateNuget" (fun _ ->      
    ["nuget/ScriptingPlugin.nuspec";
    "nuget/ScriptingPlugin.CSharp.nuspec";
    "nuget/ScriptingPlugin.CsEditor.nuspec";
    "nuget/ScriptingPlugin.FSharp.nuspec";
    "nuget/ScriptingPlugin.FsEditor.nuspec" ]
    |> List.iter (fun spec ->
    NuGet (fun p -> 
        {p with 
            Version = version     
            Authors = info.Authors                   
            PublishUrl = getBuildParamOrDefault "nugetrepo" ""
            AccessKey = getBuildParamOrDefault "keyfornuget" ""
            Publish = hasBuildParam "nugetrepo"
            OutputPath = packagesDir
        }) spec)
)

Target "AndroidPack" (fun _ ->      
    ["nuget/ScriptingPlugin.AndroidRuntime.nuspec"]
    |> List.iter (fun spec ->
    NuGet (fun p -> 
        {p with 
            Authors = info.Authors
            Project = "DualityScripting.Android"
            Version = info.Version
            Description = info.Description                                                      
            Summary = info.Description                        
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey"
            PublishUrl = getBuildParamOrDefault "nugetUrl" ""          
            OutputPath = packagesDir
        }) spec)
)

// Dependencies
"Clean"    
  ==> "SetVersions"
  ==> "RestorePackages"
  ==> "Build"
  ==> "BuildTest"
  ==> "NUnitTest"  
  ==> "CreateNuget"  

"Clean"    
  ==> "SetVersions"
  ==> "RestorePackages"
  ==> "BuildAndroid"  
  ==> "AndroidPack"  



// start build
RunTargetOrDefault "CreateNuget"
