using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.TOP.P5;

public class P3DiffuseWaveCannon : ISpecialAction
{
    public override string Name => "P3 Diffuse Wave Cannon";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31643u, 31644u, 31609u };

    public override uint WeatherID => 174u;

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

    public override void OnActionCast(ActorCastInfo info)
    {
        ushort actionId = info.ActionId;
        if ((uint)(actionId - 31643) <= 1u)
        {
            Angle angle = info.Facing + ((info.ActionId == 31643) ? 0.Degrees() : 90.Degrees());
            aoes.Add(SimpleElement.Fan(info.Pos, 100f, 120, angle, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 31609u },
                TargetHitCount = 4
            }));
            aoes.Add(SimpleElement.Fan(info.Pos, 100f, 120, angle + 180.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 31609u },
                TargetHitCount = 4
            }));
            aoes.Add(SimpleElement.Fan(info.Pos, 100f, 120, angle + 90.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 31609u },
                TargetHitCount = 4
            }));
            aoes.Add(SimpleElement.Fan(info.Pos, 100f, 120, angle - 90.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 31609u },
                TargetHitCount = 4
            }));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 31609 && aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
