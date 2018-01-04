using System.Reflection;
using System.Runtime.InteropServices;

#if NET46
[assembly: AssemblyTitle(".NET Framework 4.6")]
#endif

#if NET461
[assembly: AssemblyTitle(".NET Framework 4.6.1")]
#endif

#if NET462
[assembly: AssemblyTitle(".NET Framework 4.6.2")]
#endif

#if NET47
[assembly: AssemblyTitle(".NET Framework 4.7")]
#endif

#if NET471
[assembly: AssemblyTitle(".NET Framework 4.7.1")]
#endif

#if NETCOREAPP2_0
[assembly: AssemblyTitle(".NET Core 2.0")]
#endif

#if NETSTANDARD2_0
[assembly: AssemblyTitle(".NET Standard 2.0")]
#endif

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyCopyright("Copyright © Eben Roux 2018")]
[assembly: AssemblyProduct("Shuttle.Esb")]
[assembly: AssemblyCompany("Shuttle")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: ComVisible(false)]
