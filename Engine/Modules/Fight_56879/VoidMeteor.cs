using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_56879;

public class VoidMeteor : ISpecialAction
{
    public override string Name => "Void Meteor";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint Phase => 1u;

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon == 344)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 6f,
                radiusZ = 6f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 33967u }
                }
            }, target);
        }
    }
}
