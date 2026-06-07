using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TheMesoTerminal;

public class Bombardment : ISpecialAction
{
    private readonly List<IGameObject> terrors = new List<IGameObject>();

    public override string Name => "Bombardment";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint Phase => 3u;

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (source.BaseId == 18624)
        {
            if (id == 4563)
            {
                terrors.Add(source);
            }
            else
            {
                terrors.Remove(source);
            }
        }
    }

    public override void OnNpcYell(ulong SourceID, ushort Message)
    {
        if (terrors.Count <= 6 || Message < 18705 || Message > 18708)
        {
            return;
        }
        int count = terrors.Count;
        IGameObject yeller = SourceID.GameObject();
        Vector3 position = yeller.Position;
        bool hasNeighbor = false;
        for (int i = 0; i < count; i++)
        {
            IGameObject other = terrors[i];
            if (other != yeller && other.Position.AlmostEqual(position, 5f))
            {
                hasNeighbor = true;
                break;
            }
        }
        SimpleElement.Circle(hasNeighbor ? (position + 3.5f * yeller.Rotation.Radians().Round(1f).ToDirection()
            .ToVec3()) : position, hasNeighbor ? 14 : 3, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 43811u, 43812u }
        });
    }

    public override void Reset()
    {
        terrors.Clear();
        base.Reset();
    }
}
