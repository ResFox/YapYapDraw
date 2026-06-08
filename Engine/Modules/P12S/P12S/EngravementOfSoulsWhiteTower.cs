using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.P12S.P12S;

public class EngravementOfSoulsWhiteTower : ISpecialAction
{
    public override string Name => "Engravement of Souls (white tower)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3579)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "customCircle",
                radiusX = 3f,
                radiusZ = 3f,
                drawOnObject = true,
                refColor = new Vector4(1f, 1f, 1f, 1f),
                refTargetColor = new Vector4(1f, 1f, 1f, 1f),
                delayDrawTime = (info.Time - 4f) * 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 33548u }
                }
            }, info.TargetID.GameObject());
        }
    }
}
