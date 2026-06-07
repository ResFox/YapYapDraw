using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.CE103;

public class CommandUrn : ISpecialAction
{
    public override string Name => "CommandUrn";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        IGameObject targetObj = Svc.Objects.FirstOrDefault((IGameObject o) => o.GameObjectId == targetId);
        IGameObject actorObj = Svc.Objects.FirstOrDefault((IGameObject o) => o.GameObjectId == actorId);
        uint? baseId = ((targetObj != null) ? new uint?(targetObj.BaseId) : ((uint?)null));
        IGameObject urn = ((baseId.HasValue && baseId == 18146) ? targetObj : actorObj);
        switch (Id)
        {
        case 306u:
            SimpleElement.Circle(urn, 16f, 5000f);
            break;
        case 303u:
            SimpleElement.Circle(urn, 16f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 41420u, 39470u }
            });
            break;
        case 304u:
        {
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 41421u, 39471u }
            };
            SimpleElement.Cross(urn, 40f, 5f, default, 3000f, 0f, hitCounter);
            break;
        }
        case 305u:
            break;
        }
    }
}
