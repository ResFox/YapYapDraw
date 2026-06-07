using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.LockWyvernEx;
public class InertCrystal : ISpecialAction
{
    private bool aoesAdded;

    public override string Name => "Inert Crystal";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43926u, 43952u, 44810u, 44809u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (aoesAdded)
        {
            return;
        }
        IEnumerable<IGameObject> bigCrystals = Svc.Objects.Where((IGameObject o) => o.BaseId == 18663);
        IEnumerable<IGameObject> smallCrystals = Svc.Objects.Where((IGameObject o) => o.BaseId == 18662);
        foreach (IGameObject crystal in bigCrystals)
        {
            aoes.Add(SimpleElement.Circle(crystal, 12f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 44810u, 44809u },
                TargetHitCount = 12
            }));
        }
        foreach (IGameObject crystal in smallCrystals)
        {
            aoes.Add(SimpleElement.Circle(crystal, 6f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 44810u, 44809u },
                TargetHitCount = 12
            }));
        }
        aoesAdded = true;
    }

    public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
        IEnumerable<IGameObject> bigCrystals = Svc.Objects.Where((IGameObject o) => o.BaseId == 18663);
        IEnumerable<IGameObject> smallCrystals = Svc.Objects.Where((IGameObject o) => o.BaseId == 18662);
        if (aoesAdded || icon != 470 || !bigCrystals.Any() || !smallCrystals.Any())
        {
            return;
        }
        foreach (IGameObject crystal in bigCrystals)
        {
            aoes.Add(SimpleElement.Circle(crystal, 12f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 44810u, 44809u },
                TargetHitCount = 12
            }));
        }
        foreach (IGameObject crystal in smallCrystals)
        {
            aoes.Add(SimpleElement.Circle(crystal, 6f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 44810u, 44809u },
                TargetHitCount = 12
            }));
        }
        aoesAdded = true;
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        if (actionId - 44809 > 1)
        {
            return;
        }
        Vector3 position = info.Source.Position;
        for (int i = 0; i < aoes.Count; i++)
        {
            if (position.AlmostEqual(aoes[i].Position, 1f))
            {
                aoes[i].Remove();
                aoes.RemoveAt(i);
                if (aoes.Count == 0)
                {
                    aoesAdded = false;
                }
                break;
            }
        }
    }

    public override void Reset()
    {
        aoesAdded = false;
        base.Reset();
    }
}
