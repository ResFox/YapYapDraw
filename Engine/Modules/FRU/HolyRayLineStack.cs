using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class HolyRayLineStack : ISpecialAction
{
    public override string Name => "Holy Ray (line stack)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject source, uint icon, ulong TargetID)
    {
        if (icon == 525)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02pxf",
                radiusX = 3f,
                radiusZ = 65f,
                drawOnObject = true,
                target = TargetID.GameObject(),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40211u }
                }
            }, source);
        }
    }
}
