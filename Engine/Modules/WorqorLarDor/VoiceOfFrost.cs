using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.WorqorLarDor;

public class VoiceOfFrost : ISpecialAction
{
    public override string Name => "Voice of Frost";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36270u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "m0248_freeze_o0c",
            radiusX = 10f,
            radiusZ = 20f,
            refRotation = info.Facing,
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 36270u }
            },
            StatusCheck = new StatusCheck
            {
                CheckObject = info.SourceId.GameObject(),
                Status = 3445u,
                Reverse = true
            }
        }, info.SourceId.GameObject());
    }
}
