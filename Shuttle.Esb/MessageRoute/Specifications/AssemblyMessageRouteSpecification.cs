using System;
using System.IO;
using System.Reflection;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class AssemblyMessageRouteSpecification : TypeListMessageRouteSpecification
{
    public AssemblyMessageRouteSpecification(Assembly assembly)
    {
        AddAssemblyTypes(assembly);
    }

    public AssemblyMessageRouteSpecification(string assembly)
    {
        Assembly? scanAssembly = null;

        try
        {
            switch (Path.GetExtension(assembly))
            {
                case ".dll":
                case ".exe":
                {
                    scanAssembly = Path.GetDirectoryName(assembly) == AppDomain.CurrentDomain.BaseDirectory
                        ? Assembly.Load(Path.GetFileNameWithoutExtension(assembly))
                        : Assembly.LoadFile(assembly);
                    break;
                }
                default:
                {
                    scanAssembly = Assembly.Load(assembly);

                    break;
                }
            }
        }
        catch
        {
            // ignore
        }

        if (scanAssembly == null)
        {
            throw new MessageRouteSpecificationException(string.Format(Resources.AssemblyNotFound, assembly, "AssemblyMessageRouteSpecification"));
        }

        AddAssemblyTypes(scanAssembly);
    }

    private void AddAssemblyTypes(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            MessageTypes.Add(Guard.AgainstNullOrEmptyString(type.FullName));
        }
    }
}