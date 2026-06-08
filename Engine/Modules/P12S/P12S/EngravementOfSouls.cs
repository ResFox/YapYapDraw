using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.P12S.P12S;

public class EngravementOfSouls : ISpecialAction
{
    public override string Name => "Engravement of Souls (black tower)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3580)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "customCircle",
                radiusX = 3f,
                radiusZ = 3f,
                drawOnObject = true,
                refColor = new Vector4(0f, 0f, 0f, 1f),
                refTargetColor = new Vector4(0f, 0f, 0f, 1f),
                delayDrawTime = (info.Time - 4f) * 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 33549u }
                }
            }, info.TargetID.GameObject());
        }
    }
}
