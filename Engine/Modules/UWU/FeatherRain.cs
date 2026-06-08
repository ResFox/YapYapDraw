using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.UWU;

public class FeatherRain : ISpecialAction
{
    public override string Name => "Feather Rain";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11085u };

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        bool ok = id == 7738;
        if (ok)
        {
            uint baseId = source.BaseId;
            ok = baseId - 8722 <= 1;
        }
        if (!ok)
        {
            return;
        }
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            SimpleElement.Circle(new Vector3(allPlayer.Position.X, allPlayer.Position.Y, allPlayer.Position.Z), 3f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 11085u }
            });
        }
    }
}
