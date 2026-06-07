using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M4S;

public class ChainLightning : ISpecialAction
{
    public override string Name => "Chain Lightning";

    public override uint Phase => 7u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38426u, 38427u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(6);

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id - 279 <= 1)
        {
            DrawElement element = new DrawElement
            {
                Enable = false,
                drawAvfx = "general_1bzt",
                radiusX = 7f,
                radiusZ = 7f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38426u, 38427u },
                    TargetHitCount = 24
                }
            };
            aoes.Add(DrawManager.Draw(element, actorId.GameObject()));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
