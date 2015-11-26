namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Hyperboliq.PostgreSQL")>]
[<assembly: AssemblyDescriptionAttribute("Hyperboliq.PostgreSQL - Hyperboliq adapter for PostgreSQL")>]
[<assembly: AssemblyProductAttribute("Hyperboliq.PostgreSQL")>]
[<assembly: AssemblyCopyrightAttribute("Copyright 2015 Fredrik Forssen")>]
[<assembly: AssemblyVersionAttribute("0.1.0")>]
[<assembly: AssemblyFileVersionAttribute("0.1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1.0"
