using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_44428;

public class SusurrantBreath : ISpecialAction
{
    public override string Name => "Susurrant Breath";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36156u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "gl_fan80_o0g",
            Position = new Vector3(100f, 0f, 75f),
            drawOnObject = false,
            radiusX = 50f,
            radiusZ = 50f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        });
    }
}
