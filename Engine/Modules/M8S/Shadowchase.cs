using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M8S;

public class Shadowchase : ISpecialAction
{
    public override string Name => "Shadowchase";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (source.BaseId == 18216 && id == 4561)
        {
            Angle rotation = source.Rotation.Radians();
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 41917u }
            };
            SimpleElement.Rectangle(source, 40f, 4f, 0f, null, rotation, 3000f, 0f, hitCounter);
        }
    }
}
