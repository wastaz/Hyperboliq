#I "packages/FAKE/tools/"
#r "FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open System

let artifactsDir = "./artifacts/"
let testArtifactsDir = "./testartifacts/"
let packagingDir = "./packaging/"
let version = "0.1.0"

let testProjectFiles =
  [ "./Hyperboliq.Tests/Hyperboliq.Tests.csproj"
    "./Hyperboliq.Tests.FSharp/Hyperboliq.Tests.FSharp.fsproj"
    "./Hyperboliq.Tests.Sqllite/Hyperboliq.Tests.Sqllite.csproj"
    "./Hyperboliq.Tests.SqlServer/Hyperboliq.Tests.SqlServer.csproj"
  ]

let testAssemblies =
  [ "Hyperboliq.Tests.dll"
    "Hyperboliq.Tests.FSharp.dll"
    "Hyperboliq.Tests.Sqllite.dll"
    "Hyperboliq.Tests.SqlServer.dll"
  ] |> List.map (fun s -> testArtifactsDir + s)

Target "clean" (fun _ -> 
  trace "Clean"
  CleanDir artifactsDir
)

Target "buildTests" (fun _ ->
  testProjectFiles
  |> MSBuildDebug testArtifactsDir "Build"
  |> Log "TestBuild-Output: "
)

Target "runTests" (fun _ ->
  trace "Run tests"
  testAssemblies
  |> NUnitSequential.NUnit (fun p ->
    { p with
        StopOnError = true
        ShowLabels = true
        ToolPath = "./packages/NUnit.Runners.Net4/tools/"
    }
  )
)

Target "buildRelease" (fun _ -> 
  trace "Build Release"
  CreateFSharpAssemblyInfo "./Hyperboliq/AssemblyInfo.fs"
    [ Attribute.Title "Hyperboliq"
      Attribute.Description "Hyperboliq - Predictable SQL"
      Attribute.Product "Hyperboliq"
      Attribute.Copyright "Copyright 2015 Fredrik Forssen"
      Attribute.Version version
      Attribute.FileVersion version
    ]
  MSBuildRelease artifactsDir "Build" [ "./Hyperboliq/Hyperboliq.fsproj" ]
  |> Log "AppBuild-Output: "
)

Target "createPackage" (fun _ ->
  trace "Create Package"
  (*
  Paket.Pack(fun p ->
    { p with
        Version = version
        OutputPath = packagingDir
    }) *)
)

Target "publishPackage" (fun _ ->
  trace "Publish Package"
  (*
  Paket.Push(fun p -> { p with WorkingDir = packagingDir })
  *)
)

Target "default" DoNothing

"clean"
=?> ("buildTests", hasBuildParam "test" || hasBuildParam "publish")
=?> ("runTests", hasBuildParam "test" || hasBuildParam "test")

=?> ("buildRelease", hasBuildParam "publish")
=?> ("createPackage", hasBuildParam "publish")
=?> ("publishPackage", hasBuildParam "publish")
==> "default"

RunTargetOrDefault "default"