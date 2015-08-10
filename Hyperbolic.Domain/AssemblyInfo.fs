namespace Hyperboliq.Domain.AssemblyInfo

open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

(* General Information about an assembly is controlled through the following 
set of attributes. Change these attribute values to modify the information
associated with an assembly. *)
[<assembly: AssemblyTitle("Hyperboliq.Domain")>]
[<assembly: AssemblyDescription("")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("")>]
[<assembly: AssemblyProduct("Hyperboliq.Domain")>]
[<assembly: AssemblyCopyright("Copyright ©  2015")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]

(* Setting ComVisible to false makes the types in this assembly not visible 
to COM components.  If you need to access a type in this assembly from 
COM, set the ComVisible attribute to true on that type. *)
[<assembly: ComVisible(false)>]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[<assembly: Guid("b364b662-03a5-4700-a554-6b9e7eabd6cd")>]

(* Version information for an assembly consists of the following four values:

      Major Version
      Minor Version 
      Build Number
      Revision

You can specify all the values or you can default the Build and Revision Numbers 
by using the '*' as shown below:
[<assembly: AssemblyVersion("1.0.*")>] *)
[<assembly: AssemblyVersion("1.0.0.0")>]
[<assembly: AssemblyFileVersion("1.0.0.0")>]

[<assembly: InternalsVisibleTo("Hyperboliq.Ansi")>]
[<assembly: InternalsVisibleTo("Hyperboliq.Tests")>]
[<assembly: InternalsVisibleToAttribute("Hyperboliq.Migration.Tests")>]
do
    ()