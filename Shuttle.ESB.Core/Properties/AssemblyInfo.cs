using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET35FULL
[assembly: AssemblyTitle("Shuttle.ESB.Core for .NET Framework 3.5")]
#endif

#if NET40FULL
[assembly: AssemblyTitle("Shuttle.ESB.Core for .NET Framework 4.0")]
#endif

#if NET45FULL
[assembly: AssemblyTitle("Shuttle.ESB.Core for .NET Framework 4.5")]
#endif

#if NET451FULL
[assembly: AssemblyTitle("Shuttle.ESB.Core for .NET Framework 4.5.1")]
#endif

[assembly: AssemblyVersion("3.3.5.0")]
[assembly: InternalsVisibleTo("Shuttle.ESB.Test.Shared")]
[assembly: InternalsVisibleTo("Shuttle.ESB.Test.Integration")]
[assembly: InternalsVisibleTo("Shuttle.ESB.Test.Unit")]
[assembly: AssemblyCopyright("Copyright © Eben Roux 2010-2014")]
[assembly: AssemblyProduct("Shuttle.ESB")]
[assembly: AssemblyCompany("Shuttle")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyInformationalVersion("3.3.5")]
[assembly: ComVisible(false)]

