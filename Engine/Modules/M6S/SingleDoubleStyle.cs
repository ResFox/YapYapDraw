using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M6S;

public class SingleDoubleStyle : ISpecialAction
{
    public override string Name => "Single / Double Style";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id - 319 > 1)
        {
            return;
        }
        IGameObject actor = actorId.GameObject();
        if (actor != null)
        {
            switch (actor.BaseId)
            {
            case 18336u:
                SimpleElement.Circle(actor, 15f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 42617u }
                });
                break;
            case 18337u:
                SimpleElement.Circle((new WPos(actor.Position) + 16f * actor.Rotation.Radians().ToDirection()).ToVec3(), 15f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 42619u }
                });
                break;
            case 18338u:
            {
                Angle rotation = actor.Rotation.Radians();
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 42630u }
                };
                SimpleElement.Rectangle(actor, 60f, 3.5f, 0f, null, rotation, 3000f, 0f, hitCounter);
                break;
            }
            case 18340u:
                SimpleElement.Fan(actor, 50f, 100, actor.Rotation.Radians(), 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 42628u }
                });
                break;
            case 18341u:
                SimpleElement.Circle(actor, 30f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 42629u }
                });
                break;
            case 18339u:
                break;
            }
        }
    }
}
