using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class FaithUnmoving : ISpecialAction
{
    public override string Name => "Faith Unmoving";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25308u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        if (source != null)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "m0295_nockback_omen01i",
                radiusX = 16f,
                radiusZ = 16f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 25308u }
                }
            }, source);
        }
    }
}
