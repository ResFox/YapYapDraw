using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Valigarmanda;

public class VolcanicDrop : ISpecialAction
{
    public override string Name => "Volcanic Drop";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnEnvControl(byte index, uint state)
    {
        bool isDropIndex = (uint)(index - 14) <= 1u;
        if (isDropIndex && state == 2097168)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 20f,
                radiusZ = 20f,
                Position = new Vector3((index == 14) ? 113 : 87, 0f, 100f),
                drawOnObject = false,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 36838u },
                    TargetHitCount = 5
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer);
        }
    }
}
