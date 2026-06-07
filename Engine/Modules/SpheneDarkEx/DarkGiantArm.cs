using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.SpheneDarkEx;
public class DarkGiantArm : ISpecialAction
{
    public override string Name => "Dark Giant Arm";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 102)
        {
            IGameObject source = actorId.GameObject();
            SimpleElement.Rectangle(source.Position, 36f, 5f, 0f, source.Rotation.Radians(), 12300f);
        }
    }
}
