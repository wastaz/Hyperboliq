namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Hyperboliq")>]
[<assembly: AssemblyDescriptionAttribute("Hyperboliq - Predictable SQL")>]
[<assembly: AssemblyProductAttribute("Hyperboliq")>]
[<assembly: AssemblyCopyrightAttribute("Copyright 2015 Fredrik Forssen")>]
[<assembly: AssemblyVersionAttribute("0.1.0")>]
[<assembly: AssemblyFileVersionAttribute("0.1.0")>]

do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1.0"
