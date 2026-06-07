using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Valigarmanda;

public class ArcaneLightning : ISpecialAction
{
    public override string Name => "Arcane Lightning";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        if (GameObject.BaseId == 16770)
        {
            Angle rotation = GameObject.Rotation.Radians();
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 39002u }
            };
            SimpleElement.Rectangle(GameObject, 50f, 2.5f, 0f, null, rotation, 3000f, 0f, hitCounter);
        }
    }
}
