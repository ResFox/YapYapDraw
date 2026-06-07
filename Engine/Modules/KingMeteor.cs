using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M11S;

public class KingMeteor : ISpecialAction
{
    private readonly List<ulong> actors = new List<ulong>();

    public override string Name => "King Meteor";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46144u, 46147u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        switch (info.ActionId)
        {
        case 46144u:
            base.CanDraw = true;
            break;
        case 46147u:
            actors.Clear();
            break;
        }
    }

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        bool isMeteorTether = Id == 57 || Id == 249;
        if (isMeteorTether && base.CanDraw && !actors.Contains(targetId))
        {
            actors.Add(targetId);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 5f,
                radiusZ = 60f,
                drawOnObject = true,
                target = targetId.GameObject(),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 46147u }
                }
            }, actorId.GameObject());
        }
    }

    public override void Reset()
    {
        actors.Clear();
        base.Reset();
    }
}
