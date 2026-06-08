using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.UWU;

public class CrimsonCyclone : ISpecialAction
{
    public override string Name => "P4 Crimson Cyclone";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11126u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 11126)
        {
            base.CanDraw = true;
        }
    }

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (base.CanDraw && source.BaseId == 8730 && id == 7747)
        {
            base.CanDraw = false;
            Angle rotation = source.Rotation.Radians();
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 11103u }
            };
            SimpleElement.Rectangle(source, 49f, 9f, 5f, null, rotation, 3000f, 0f, hitCounter);
            ActionQueue.Enqueue((new HashSet<uint> { 11103u }, delegate
            {
                DrawElement obj = new DrawElement
                {
                    drawAvfx = "general_x02f",
                    Position = new Vector3(100f, 0f, 100f),
                    drawOnObject = false,
                    radiusX = 5f,
                    radiusZ = 20f,
                    fixRotation = true,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 11104u }
                    }
                };
                DrawManager.Draw(obj, (IGameObject?)Svc.Objects.LocalPlayer);
                obj.refRotation = 90.Degrees();
                DrawManager.Draw(obj, (IGameObject?)Svc.Objects.LocalPlayer);
            }));
        }
    }
}
