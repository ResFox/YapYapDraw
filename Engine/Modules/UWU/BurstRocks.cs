using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.UWU;

public class BurstRocks : ISpecialAction
{
    public override string Name => "Burst (rocks)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnObjectCreatedEvent(IGameObject gameObject)
    {
        if (gameObject.BaseId == 8728)
        {
            SimpleElement.Circle(gameObject, 6.3f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 11114u }
            });
        }
    }
}
