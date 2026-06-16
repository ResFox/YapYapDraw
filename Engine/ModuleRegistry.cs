using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Engine;

public static class ModuleRegistry
{
    public sealed record LoadedFight(BaseModule Host, List<ISpecialAction> Mechanics);

    internal static IReadOnlyList<ISpecialAction> AllMechanics { get; private set; } = Array.Empty<ISpecialAction>();

    public static IReadOnlyList<LoadedFight> LoadAll()
    {
        var asm = typeof(ModuleRegistry).Assembly;
        var hosts = asm.GetTypes()
            .Where(t => t is { IsAbstract: false, IsClass: true }
                        && typeof(DrawModule).IsAssignableFrom(t)
                        && t != typeof(DrawModule)
                        && t != typeof(BaseModule))
            .OrderBy(t => t.Namespace, StringComparer.Ordinal)
            .ThenBy(t => t.Name, StringComparer.Ordinal)
            .ToList();

        var result = new List<LoadedFight>();
        foreach (var hostType in hosts)
        {
            try
            {
                if (Activator.CreateInstance(hostType) is not DrawModule host)
                    continue;

                var ns = hostType.Namespace;
                if (string.IsNullOrEmpty(ns))
                    continue;

                var mechanics = asm.GetTypes()
                    .Where(t => t is { IsAbstract: false, IsClass: true }
                                && typeof(ISpecialAction).IsAssignableFrom(t)
                                && t.Namespace != null
                                && (t.Namespace == ns || t.Namespace.StartsWith(ns + ".", StringComparison.Ordinal)))
                    .OrderBy(t => t.Namespace, StringComparer.Ordinal)
                    .ThenBy(t => t.Name, StringComparer.Ordinal)
                    .Select(t =>
                    {
                        try { return Activator.CreateInstance(t) as ISpecialAction; }
                        catch { return null; }
                    })
                    .Where(m => m != null)
                    .Cast<ISpecialAction>()
                    .Where(m => m.Registered)
                    .ToList();

                foreach (var m in mechanics)
                {
                    try { m.Setup(); } catch { }
                    host.SpecialActions.Add(m);
                }

                result.Add(new LoadedFight(host, mechanics));
            }
            catch
            {
                // skip broken host
            }
        }

        AllMechanics = result.SelectMany(f => f.Mechanics).ToList();
        return result;
    }
}
