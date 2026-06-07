using System;
using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Engine.Util;

public static class ModuleUtil
{
    public static T? GetSpecialAction<T>() where T : ISpecialAction
        => ModuleRegistry.AllMechanics.OfType<T>().FirstOrDefault();

    public static void SortBy<TValue, TKey>(this List<TValue> list, Func<TValue, TKey> proj) where TKey : notnull, IComparable
    {
        list.Sort((l, r) => proj(l).CompareTo(proj(r)));
    }
}
