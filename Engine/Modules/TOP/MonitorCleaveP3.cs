using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class MonitorCleaveP3 : ISpecialAction
{
    public override string Name => "Monitor Cleave (P3)";

    public override uint Phase => 3u;

    public override uint WeatherID => 79u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31595u, 31596u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "gl_fan180_1bf",
            radiusX = 100f,
            radiusZ = 100f,
            drawOnObject = true,
            refRotation = info.ActionId == 31595u ? 90.Degrees() : -90.Degrees(),
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        }, info.SourceId.GameObject());
    }
}
