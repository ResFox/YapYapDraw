using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.CloudOfDarkness;

public class Rapid_sequenceParticleBeam : ISpecialAction
{
    public override string Name => "Rapid-sequence Particle Beam (line stack)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40512u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02pxf",
            radiusX = 3f,
            radiusZ = 50f,
            drawOnObject = true,
            target = (IGameObject?)Svc.Objects.LocalPlayer,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 40514u },
                TargetHitCount = 12
            }
        }, info.SourceId.GameObject());
        DrawManager.Draw(new DrawElement
        {
            drawType = ElementType.LockOn,
            drawAvfx = "share_laser_8sec_0t"
        }, (IGameObject?)Svc.Objects.LocalPlayer, info.SourceId.GameObject());
    }
}
