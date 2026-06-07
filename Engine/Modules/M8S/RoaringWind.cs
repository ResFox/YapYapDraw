using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M8S;

public class RoaringWind : ISpecialAction
{
    public override string Name => "Roaring Wind";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42890u };

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (source.BaseId == 18527 && id == 4562)
        {
            Angle rotation = source.Rotation.Radians();
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 42890u }
            };
            SimpleElement.Rectangle(source, 40f, 4f, 0f, null, rotation, 3000f, 0f, hitCounter);
        }
    }
}
