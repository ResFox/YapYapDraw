using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.E11;

public class FellruinTrailLine : ISpecialAction
{
    public override string Name => "Fellruin Trail (line)";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint Phase => 1u;

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 1678)
        {
            IGameObject target = info.TargetID.GameObject();
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 8f,
                radiusZ = 50f,
                drawOnObject = true,
                refRotation = target.Rotation.Radians(),
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 22078u }
                }
            }, target);
        }
    }
}
