using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TOP;

public class P1KnockbackPredict : ISpecialAction
{
    public override string Name => "P1 Knockback (predict)";

    public override uint Phase => 2u;

    public override uint WeatherID => 78u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31550u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        base.CanDraw = true;
    }

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon == 100 && base.CanDraw)
        {
            base.CanDraw = false;
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "e5d1_b1_kblaser_t1",
                radiusX = 1f,
                radiusZ = 13f,
                drawOnObject = true,
                KnockBackCheck = new KnockBackCheck
                {
                    OriginPos = new Vector3(100f, 0f, 100f)
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31534u }
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer);
        }
    }
}
