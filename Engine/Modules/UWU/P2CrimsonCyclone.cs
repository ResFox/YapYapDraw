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

public class P2CrimsonCyclone : ISpecialAction
{
    public override string Name => "P2 Crimson Cyclone";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11596u, 11103u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 11596)
        {
            base.CanDraw = true;
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 11103 && base.CanDraw)
        {
            base.CanDraw = false;
            ActionQueue.Enqueue((new HashSet<uint> { 11103u }, action));
        }
        static void action()
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
        }
    }
}
