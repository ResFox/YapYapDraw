using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.ShishuDeep;

public class FairyPellyEWPhantomDance : ISpecialAction
{
    private readonly List<IGameObject> left = new List<IGameObject>();

    private readonly List<IGameObject> right = new List<IGameObject>();

    public override string Name => "Fairy Pelly E/W Phantom Dance";

    public override HashSet<uint> ActionID
    {
        get
        {
            HashSet<uint> ids = new HashSet<uint>();
            ids.Add(45428u);
            ids.Add(45429u);
            ids.Add(46946u);
            ids.Add(46947u);
            foreach (uint id in carpetRushIds)
            {
                ids.Add(id);
            }
            return ids;
        }
    }

    private static HashSet<uint> carpetRushIds => new HashSet<uint> { 45432u, 45433u, 45442u, 45443u, 46573u, 46574u, 46950u, 46951u, 47020u, 47021u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 45428:
        case 46946:
            right.Add(info.SourceId.GameObject());
            break;
        case 45429:
        case 46947:
            left.Add(info.SourceId.GameObject());
            break;
        }
        base.NumCasts = 0;
    }

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 355 && (left.Count != 0 || right.Count != 0))
        {
            base.NumCasts++;
            bool isLeft = left.Any((IGameObject o) => o.Position.AlmostEqual(actorId.GameObject().Position, 1f));
            DrawElement element = new DrawElement
            {
                drawAvfx = "general02xf",
                Position = actorId.GameObject().Position,
                drawOnObject = false,
                targetPosition = targetId.GameObject().Position,
                radiusX = 40f,
                radiusZ = 40f,
                refOffsetZ = -40f,
                refOffsetRotation = (isLeft ? 90.Degrees() : (-90.Degrees())),
                hitCounter = new HitCounter
                {
                    ActionID = carpetRushIds,
                    TargetHitCount = base.NumCasts
                }
            };
            aoes.Add(DrawManager.Draw(element));
            if (isLeft)
            {
                left.Add(targetId.GameObject());
            }
            else
            {
                right.Add(targetId.GameObject());
            }
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (carpetRushIds.Contains(info.ActionId))
        {
            if (aoes.Count > 0)
            {
                aoes[0].Remove();
                aoes.RemoveAt(0);
            }
            left.Clear();
            right.Clear();
            base.NumCasts = 0;
        }
    }

    public override void Reset()
    {
        left.Clear();
        right.Clear();
        base.Reset();
    }
}
