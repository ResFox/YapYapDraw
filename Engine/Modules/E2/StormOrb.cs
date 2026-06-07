using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.E2;

public class StormOrb : ISpecialAction
{
    public override string Name => "Storm Orb";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        if (GameObject.BaseId == 11537)
        {
            SimpleElement.Circle(GameObject, 8f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 19426u }
            });
        }
    }
}
