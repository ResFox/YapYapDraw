using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_56879;

public class DoubleMeteor : ISpecialAction
{
    public override string Name => "Double Meteor";

    public override HashSet<uint> ActionID => new HashSet<uint> { 34699u, 33974u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            Position = info.Pos,
            drawOnObject = false,
            radiusX = 20f,
            radiusZ = 20f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
