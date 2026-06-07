using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class CauterizeH : ISpecialAction
{
    public override string Name => "Cauterize";

    public override uint Phase => 6u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 27967u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        if (source != null)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02pxf",
                radiusX = 11f,
                radiusZ = 80f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 27967u }
                }
            }, source);
        }
    }
}
