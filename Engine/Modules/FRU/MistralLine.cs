using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class MistralLine : ISpecialAction
{
    public override string Name => "Mistral (line)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40158u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject partner = Svc.Objects.SearchById((ulong)(info.SourceId + 1));
        if (partner != null)
        {
            Angle rotation = partner.Rotation.Radians();
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 40157u }
            };
            SimpleElement.Rectangle(partner, 50f, 8f, 0f, null, rotation, 3000f, 0f, hitCounter);
        }
    }
}
