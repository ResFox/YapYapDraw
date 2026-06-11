using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.DancingMad.P1;

public class ChainTrap : ISpecialAction
{
    private readonly Dictionary<StaticVfx, IGameObject> _knockbackSource = new Dictionary<StaticVfx, IGameObject>();

    public override string Name => "Chain Trap";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void Update()
    {
        if (aoes.Count == 0)
            return;

        IGameObject? lp = Svc.Objects.LocalPlayer;
        if (lp == null)
            return;

        foreach (StaticVfx aoe in aoes)
        {
            if (_knockbackSource.TryGetValue(aoe, out IGameObject? source) && source != null)
                aoe.Enable = new WPos(lp.Position).InCircle(new WPos(source.Position), 6f);
            else
                aoe.Enable = false;
        }
    }

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 5078)
            return;

        IGameObject? target = info.TargetID.GameObject();
        IGameObject? lp = Svc.Objects.LocalPlayer;
        if (target == null || lp == null)
            return;

        if (!SameRoleBucket(lp, target))
            return;

        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "nockback_omen04t1",
            radiusX = 6f,
            radiusZ = 6f,
            destroyTime = 5000f,
            delayDrawTime = (info.Time - 5f) * 1000f,
            StatusCheck = new StatusCheck
            {
                CheckObject = target,
                Status = 5078u
            }
        }, target);

        if (info.TargetID != lp.GameObjectId)
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "e5d1_b1_kblaser_t1",
                radiusX = 1f,
                radiusZ = 14f,
                destroyTime = 5000f,
                delayDrawTime = (info.Time - 5f) * 1000f,
                target = target,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47783u }
                }
            };
            StaticVfx vfx = DrawManager.Draw(element, lp);
            aoes.Add(vfx);
            _knockbackSource[vfx] = target;
        }
    }

    public override void Reset()
    {
        _knockbackSource.Clear();
        base.Reset();
    }

    private static bool SameRoleBucket(IGameObject a, IGameObject b)
    {
        if (a is not IPlayerCharacter pa || b is not IPlayerCharacter pb)
            return false;

        bool aDps = ((ICharacter)pa).GetRole() == CombatRole.DPS;
        bool bDps = ((ICharacter)pb).GetRole() == CombatRole.DPS;
        return aDps == bDps;
    }
}
