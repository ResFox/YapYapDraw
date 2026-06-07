using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class AscalonsMercyRevealed : ISpecialAction
{
    public override string Name => "Ascalon's Mercy Revealed";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25546u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IEnumerable<IGameObject> players = Svc.Objects.Where((IGameObject obj) => (int)obj.ObjectKind == 1);
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        if (source == null)
        {
            return;
        }
        foreach (IGameObject item in players)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan030_1bf",
                radiusX = 50f,
                radiusZ = 50f,
                drawOnObject = true,
                target = item,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 25547u }
                }
            }, source);
        }
    }
}
