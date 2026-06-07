using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Valigarmanda;

public class SpikesicleBait : ISpecialAction
{
    public override string Name => "Spikesicle (bait)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnEnvControl(byte index, uint state)
    {
        bool isBaitState = state == 131076;
        if (isBaitState)
        {
            bool isBaitIndex = (uint)(index - 4) <= 1u;
            isBaitState = isBaitIndex;
        }
        if (isBaitState && base.NumCasts <= 0)
        {
            base.NumCasts++;
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "movepos_mark_01t",
                Position = new Vector3((index == 4) ? 95.27f : 104.73f, 0f, 95.83f),
                drawOnObject = false,
                radiusX = 30f,
                radiusY = 5f,
                radiusZ = 30f,
                destroyTime = 12000f
            }, (IGameObject?)Svc.Objects.LocalPlayer);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "movepos_mark_01t",
                Position = new Vector3((index == 4) ? 103.28f : 96.72f, 0f, 101.77f),
                drawOnObject = false,
                radiusX = 30f,
                radiusY = 5f,
                radiusZ = 30f,
                destroyTime = 7000f,
                delayDrawTime = 12000f
            }, (IGameObject?)Svc.Objects.LocalPlayer);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "movepos_mark_01t",
                Position = new Vector3(100f, 0f, 99.34f),
                drawOnObject = false,
                radiusX = 30f,
                radiusY = 5f,
                radiusZ = 30f,
                delayDrawTime = 19000f,
                destroyTime = 8000f
            }, (IGameObject?)Svc.Objects.LocalPlayer);
        }
    }
}
