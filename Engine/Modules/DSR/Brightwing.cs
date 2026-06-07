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

public class Brightwing : ISpecialAction
{
    public override string Name => "Brightwing";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25316u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        if (source == null)
        {
            return;
        }
        foreach (IGameObject item in Svc.Objects.Where((IGameObject obj) => (int)obj.ObjectKind == 1).ToList())
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan030_1bf",
                radiusX = 18f,
                radiusZ = 18f,
                drawOnObject = true,
                target = item,
                distanceCheck = new DistanceCheck
                {
                    CheckObject = source,
                    CheckType = 0,
                    Count = 2
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 25369u },
                    TargetHitCount = 8
                }
            }, source);
        }
    }
}
