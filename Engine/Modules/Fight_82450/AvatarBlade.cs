using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_82450;

public class AvatarBlade : ISpecialAction
{
    public override string Name => "Avatar Blade";

    public override HashSet<uint> ActionID => new HashSet<uint> { 35970u, 35972u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 35970)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 60f,
                radiusZ = 60f,
                drawOnObject = true,
                refOffsetZ = 33f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { info.ActionId }
                }
            }, info.SourceId.GameObject());
        }
        if (info.ActionId == 35972)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_sircle_2010bf",
                radiusX = 96f,
                radiusZ = 96f,
                drawOnObject = true,
                refOffsetZ = 33f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { info.ActionId }
                }
            }, info.SourceId.GameObject());
        }
    }
}
