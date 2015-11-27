namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Hyperboliq.SqlServer")>]
[<assembly: AssemblyDescriptionAttribute("Hyperboliq.SqlServer - Hyperboliq adapter for SQL Server")>]
[<assembly: AssemblyProductAttribute("Hyperboliq.SqlServer")>]
[<assembly: AssemblyCopyrightAttribute("Copyright 2015 Fredrik Forssen")>]
[<assembly: AssemblyVersionAttribute("0.2.0")>]
[<assembly: AssemblyFileVersionAttribute("0.2.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.2.0"
