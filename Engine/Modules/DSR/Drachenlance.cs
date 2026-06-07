using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class Drachenlance : ISpecialAction
{
    public override string Name => "Drachenlance";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 26379u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        if (source != null)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan090_1bf",
                radiusX = 13f,
                radiusZ = 13f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 26380u }
                }
            }, source);
        }
    }
}
