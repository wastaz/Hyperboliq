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

let createNugetPackage templateFile =
  Paket.Pack (fun p ->
    { p with
        TemplateFile = templateFile
        Version = version
        OutputPath = packagingDir
    })

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

Target "buildSqlLite" (fun _ ->
  trace "Building SqlLite adapter..."
  CreateFSharpAssemblyInfo "./Hyperboliq.Sqllite/AssemblyInfo.fs"
    [ Attribute.Title "Hyperboliq.SqlLite"
      Attribute.Description "Hyperboliq.SqlLite- Hyperboliq adapter for SqlLite"
      Attribute.Product "Hyperboliq.SqlLite"
      Attribute.Copyright "Copyright 2015 Fredrik Forssen"
      Attribute.Version version
      Attribute.FileVersion version
    ]
  MSBuildRelease "./Hyperboliq.SqlLite/bin/Release" "Build" [ "./Hyperboliq.SqlLite/Hyperboliq.SqlLite.fsproj" ]
  |> Log "AppBuild-Output: "
)

Target "buildPostgres" (fun _ -> 
  trace "Building Postgres adapter..."
  CreateFSharpAssemblyInfo "./Hyperboliq.Postgres/AssemblyInfo.fs"
    [ Attribute.Title "Hyperboliq.PostgreSQL"
      Attribute.Description "Hyperboliq.PostgreSQL - Hyperboliq adapter for PostgreSQL"
      Attribute.Product "Hyperboliq.PostgreSQL"
      Attribute.Copyright "Copyright 2015 Fredrik Forssen"
      Attribute.Version version
      Attribute.FileVersion version
    ]
  MSBuildRelease "./Hyperboliq.Postgres/bin/Release" "Build" [ "./Hyperboliq.Postgres/Hyperboliq.Postgres.fsproj" ]
  |> Log "AppBuild-Output: "
)

Target "buildSqlServer" (fun _ -> 
  trace "Building Sql Server adapter..."
  CreateFSharpAssemblyInfo "./Hyperboliq.SqlServer/AssemblyInfo.fs"
    [ Attribute.Title "Hyperboliq.SqlServer"
      Attribute.Description "Hyperboliq.SqlServer - Hyperboliq adapter for SQL Server"
      Attribute.Product "Hyperboliq.SqlServer"
      Attribute.Copyright "Copyright 2015 Fredrik Forssen"
      Attribute.Version version
      Attribute.FileVersion version
    ]
  MSBuildRelease "./Hyperboliq.SqlServer/bin/Release" "Build" [ "./Hyperboliq.SqlServer/Hyperboliq.SqlServer.fsproj" ]
  |> Log "AppBuild-Output: "
)

Target "buildCore" (fun _ -> 
  trace "Build Release"
  CreateFSharpAssemblyInfo "./Hyperboliq.Ansi/AssemblyInfo.fs"
    [ Attribute.Title "Hyperboliq"
      Attribute.Description "Hyperboliq - Predictable SQL"
      Attribute.Product "Hyperboliq"
      Attribute.Copyright "Copyright 2015 Fredrik Forssen"
      Attribute.Version version
      Attribute.FileVersion version
    ]
  MSBuildRelease "./Hyperboliq.Ansi/bin/Release" "Build" [ "./Hyperboliq.Ansi/Hyperboliq.Ansi.fsproj" ]
  |> Log "AppBuild-Output: "
)

Target "createSqlLitePackage" (fun _ ->
  trace "Create SqlLite package..."
  createNugetPackage "./Hyperboliq.Sqllite/paket.template"
)

Target "createPostgresPackage" (fun _ ->
  trace "Create Postgres package..."
  createNugetPackage "./Hyperboliq.Postgres/paket.template"
)

Target "createSqlServerPackage" (fun _ ->
  trace "Create Sql Server package..."
  createNugetPackage "./Hyperboliq.SqlServer/paket.template"
)

Target "createCorePackage" (fun _ ->
  trace "Create Package"
  createNugetPackage "./Hyperboliq.Ansi/paket.template"
)

Target "publishPackages" (fun _ ->
  trace "Publish Package"
  if hasBuildParam "publish" then
    Paket.Push(fun p -> { p with WorkingDir = packagingDir })
  else
    trace "IMPORTANT!"
    trace "To actually publish, add the build parameter \"publish\". Otherwise nothing will be published."
)

Target "createPackages" DoNothing

Target "default" DoNothing

"createCorePackage" ==> "createPackages"
"createSqlServerPackage" ==> "createPackages"
"createPostgresPackage" ==> "createPackages"
"createSqlLitePackage" ==> "createPackages"

"buildCore" ==> "createCorePackage"
"buildSqlServer" ==> "createSqlServerPackage"
"buildPostgres" ==> "createPostgresPackage"
"buildSqlLite" ==> "createSqlLitePackage"

"clean"
==> "buildTests"
==> "runTests"
==> "default"


"runTests"
==> "createPackages"
==> "publishPackages"


RunTargetOrDefault "default"