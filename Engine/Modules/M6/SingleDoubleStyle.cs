using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M6;

public class SingleDoubleStyle : ISpecialAction
{
    private bool target;

    public override string Name => "Single / Double Style";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42581u, 42583u, 42585u };

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if ((Id - 319 > 1 && Id != 324) || 1 == 0)
        {
            return;
        }
        IGameObject actor = actorId.GameObject();
        switch (actor.BaseId)
        {
        case 18330u:
            SimpleElement.Circle(actor.Position, 15f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 42581u }
            });
            break;
        case 18331u:
            SimpleElement.Circle(actor.Position + 16f * actor.Rotation.Radians().ToDirection().ToVec3(), 15f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 42583u }
            });
            break;
        case 18333u:
            if (target)
            {
                SimpleElement.RectangleToTarget(actor.Position, new Vector3(100f, 0f, 100f), 60f, 3.5f, 3000f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 42585u }
                });
            }
            else
            {
                aoes.Add(SimpleElement.Rectangle(actor.Position, 60f, 3.5f, 0f, actor.Rotation.Radians(), 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 42585u }
                }));
            }
            break;
        case 18332u:
            target = true;
            {
                foreach (StaticVfx aoe in aoes)
                {
                    aoe.TargetPosition = new Vector3(100f, 0f, 100f);
                }
                break;
            }
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        target = false;
        aoes.Clear();
    }
}
