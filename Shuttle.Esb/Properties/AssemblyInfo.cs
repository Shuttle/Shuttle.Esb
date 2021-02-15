using System.Reflection;
using System.Runtime.InteropServices;

#if NETFRAMEWORK
[assembly: AssemblyTitle(".NET Framework")]
#endif

#if NETCOREAPP
[assembly: AssemblyTitle(".NET Core")]
#endif

#if NETSTANDARD
[assembly: AssemblyTitle(".NET Standard")]
#endif

[assembly: AssemblyVersion("11.2.0.0")]
[assembly: AssemblyCopyright("Copyright (c) 2021, Eben Roux")]
[assembly: AssemblyProduct("Shuttle.Esb")]
[assembly: AssemblyCompany("Eben Roux")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyInformationalVersion("11.2.0")]
[assembly: ComVisible(false)]