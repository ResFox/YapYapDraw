using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4S;

public class FlameSlash : ISpecialAction
{
    public override string Name => "Flame Slash";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38342u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject add in Svc.Objects.Where((IGameObject o) => o.BaseId == 17325))
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 2.5f,
                radiusZ = 60f,
                drawOnObject = true,
                target = add,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38343u },
                    TargetHitCount = 6
                }
            }, info.SourceId.GameObject());
        }
    }
}
