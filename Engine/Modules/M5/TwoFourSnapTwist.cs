using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M5;

public class TwoFourSnapTwist : ISpecialAction
{
    private static readonly HashSet<uint> TwoSnapTwistFirst = new HashSet<uint> { 42704u, 42701u, 42699u, 42702u, 42197u, 42700u, 42703u, 42198u };

    private static readonly HashSet<uint> FourSnapTwistFirst = new HashSet<uint> { 42719u, 42720u, 42716u, 42718u, 42201u, 42717u, 42721u, 42202u };

    public override string Name => "Two / Four Snap Twist";

    public override HashSet<uint> ActionID
    {
        get
        {
            HashSet<uint> ids = new HashSet<uint>();
            foreach (uint id in TwoSnapTwistFirst)
            {
                ids.Add(id);
            }
            ids.Add(42705u);
            ids.Add(42724u);
            foreach (uint id in FourSnapTwistFirst)
            {
                ids.Add(id);
            }
            ids.Add(42706u);
            ids.Add(42725u);
            return ids;
        }
    }

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public override void OnActionCast(ActorCastInfo info)
    {
        if (TwoSnapTwistFirst.Contains(info.ActionId) || FourSnapTwistFirst.Contains(info.ActionId))
        {
            aoes.Add(SimpleElement.Rectangle(info.Pos, 20f, 20f, 0f, info.Facing, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 42705u, 42724u }
            }));
            aoes.Add(SimpleElement.Rectangle(info.Pos, 20f, 20f, 0f, info.Facing + 180.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 42706u, 42725u }
            }));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        bool isResolve = actionId - 42705 <= 1 || actionId - 42724 <= 1;
        if (isResolve && aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
