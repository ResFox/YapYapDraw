using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_44428;

public class StranglingCoil : ISpecialAction
{
    public override string Name => "Strangling Coil";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36160u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "gl_sircle_3008bf",
            Position = new Vector3(100f, 0f, 100f),
            drawOnObject = false,
            radiusX = 30f,
            radiusZ = 30f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        });
    }
}
