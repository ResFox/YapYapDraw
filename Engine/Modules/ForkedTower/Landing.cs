using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.ForkedTower;

public class Landing : ISpecialAction
{
    public override string Name => "Landing";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41709u, 41812u, 43293u, 43294u, 43794u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 41709:
            SimpleElement.Circle(info.Pos, 18f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            });
            break;
        case 41812:
            SimpleElement.Rectangle(info.Pos, 6f, 15f, 0f, info.Facing, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            });
            break;
        case 43293:
        case 43794:
            SimpleElement.Rectangle(info.Pos, 15f, 15f, 0f, info.Facing, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            });
            break;
        case 43294:
            if (ModuleUtil.GetSpecialAction<RayofIgnorance>().aoes.Count > 0)
            {
                Angle rotation = ModuleUtil.GetSpecialAction<RayofIgnorance>().aoes[0].Rotation;
                if (info.Facing == rotation)
                {
                    break;
                }
            }
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "m0532_hukitobashi_o0w",
                Position = info.Pos,
                radiusX = 15f,
                radiusZ = 30f,
                drawOnObject = false,
                refRotation = info.Facing,
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { info.ActionId }
                }
            });
            break;
        }
    }
}
