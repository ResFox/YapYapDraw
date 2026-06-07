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

namespace YapYapDraw.Modules.TEA;

public class PropellerWind : ISpecialAction
{
    public override string Name => "Propeller Wind";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18482u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject propeller = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 11393);
        if (propeller != null)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan060_1bpf",
                radiusX = 50f,
                radiusZ = 50f,
                target = propeller,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 18482u }
                }
            }, info.SourceId.GameObject());
        }
    }
}
