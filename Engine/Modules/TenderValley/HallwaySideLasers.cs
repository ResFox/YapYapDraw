using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TenderValley;

public class HallwaySideLasers : ISpecialAction
{
    private enum Type
    {
        Short,
        Medium,
        Long
    }

    private static readonly (Vector2 Position, Type Type)[] AOEMap = new(Vector2, Type)[4]
    {
        (new Vector2(-112.5f, -486.5f), Type.Medium),
        (new Vector2(-147.5f, -471.5f), Type.Medium),
        (new Vector2(-147.5f, -486.5f), Type.Short),
        (new Vector2(-112.5f, -471.5f), Type.Short)
    };

    public override string Name => "Hallway Side Lasers";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint Phase => 3u;

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 37)
        {
            IGameObject source = actorId.GameObject();
            switch (GetType(new Vector2(source.Position.X, source.Position.Z)) ?? Type.Long)
            {
            case Type.Short:
            {
                Angle rotation = source.Rotation.Radians();
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 39823u, 39824u, 39825u }
                };
                SimpleElement.Rectangle(source, 12f, 4f, 0f, null, rotation, 3000f, 0f, hitCounter);
                break;
            }
            case Type.Medium:
            {
                Angle rotation = source.Rotation.Radians();
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 39823u, 39824u, 39825u }
                };
                SimpleElement.Rectangle(source, 22f, 4f, 0f, null, rotation, 3000f, 0f, hitCounter);
                break;
            }
            case Type.Long:
            {
                Angle rotation = source.Rotation.Radians();
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 39823u, 39824u, 39825u }
                };
                SimpleElement.Rectangle(source, 35f, 4f, 0f, null, rotation, 3000f, 0f, hitCounter);
                break;
            }
            }
        }
    }

    private static Type? GetType(Vector2 position)
    {
        (Vector2, Type)[] map = AOEMap;
        for (int i = 0; i < map.Length; i++)
        {
            var (pos, value) = map[i];
            if (position.AlmostEqual(pos, 1f))
            {
                return value;
            }
        }
        return null;
    }
}
