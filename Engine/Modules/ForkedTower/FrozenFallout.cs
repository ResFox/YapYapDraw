using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.ForkedTower;

public class FrozenFallout : ISpecialAction
{
    public override string Name => "Frozen Fallout";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42463u, 42464u, 42459u };

    public override uint Phase => 2u;

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

    public override void OnActionCast(ActorCastInfo info)
    {
        ushort actionId = info.ActionId;
        if ((uint)(actionId - 42463) <= 1u)
        {
            switch (info.ActionId)
            {
            case 42463:
            {
                DrawElement element = new DrawElement
                {
                    drawAvfx = "m0087_red_o01h",
                    Position = info.SourceId.GameObject().Position,
                    drawOnObject = false,
                    radiusX = 22f,
                    radiusZ = 22f,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 42459u },
                        TargetHitCount = 8
                    }
                };
                aoes.Add(DrawManager.Draw(element));
                break;
            }
            case 42464:
            {
                DrawElement element = new DrawElement
                {
                    drawAvfx = "m0087_blue_o01h",
                    Position = info.SourceId.GameObject().Position,
                    drawOnObject = false,
                    radiusX = 22f,
                    radiusZ = 22f,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 42459u },
                        TargetHitCount = 8
                    }
                };
                aoes.Add(DrawManager.Draw(element));
                break;
            }
            }
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 42459 && aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
