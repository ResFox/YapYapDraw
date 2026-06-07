using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class HotWingTail : ISpecialAction
{
    public override string Name => "Hot Wing / Hot Tail";

    public override uint Phase => 6u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 27947u, 27949u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        if (source != null)
        {
            if (info.ActionId == 27947)
            {
                DrawElement element = new DrawElement
                {
                    drawAvfx = "general02xf",
                    radiusX = 10.5f,
                    radiusZ = 50f,
                    refOffsetX = 14.5f,
                    drawOnObject = true,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 27947u }
                    }
                };
                DrawManager.Draw(element, source);
                element.refOffsetX = -14.5f;
                DrawManager.Draw(element, source);
            }
            else
            {
                SimpleElement.Rectangle(info, 50f, 8f);
            }
        }
    }
}
