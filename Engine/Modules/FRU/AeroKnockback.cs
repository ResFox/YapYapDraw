using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.FRU;

public class AeroKnockback : ISpecialAction
{
    public override string Name => "Aero (knockback)";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2463 && info.TargetID == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
        {
            DrawElement trap = new DrawElement
            {
                drawAvfx = "m0119_trap_02t",
                Position = new Vector3(88.2f, 0f, 115.2f),
                drawOnObject = false,
                radiusX = 1.5f,
                radiusY = 5f,
                radiusZ = 1.5f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40241u }
                }
            };
            DrawManager.Draw(trap, (IGameObject?)Svc.Objects.LocalPlayer);
            trap.Position = new Vector3(112.2f, 0f, 115.2f);
            DrawManager.Draw(trap, (IGameObject?)Svc.Objects.LocalPlayer);
            SimpleElement.ShowText("Wind — wait bottom-left / bottom-right", (TextGimmickHintStyle)0);
        }
    }
}
