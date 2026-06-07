using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M3S;

public class BrutalImpactKnockback : ISpecialAction
{
    private enum State
    {
        None,
        Ready,
        NextNS,
        NextEW,
        NextCorners,
        NextCenter,
        Done
    }

    private State curState;

    public override string Name => "Brutal Impact (knockback)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37883u, 38542u, 37884u, 38543u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Where(aoe =>
    {
        IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
        return localPlayer != null && ((IGameObject)localPlayer).Position.AlmostEqual(aoe.KnockBackCheck.OriginPos.Value, 4f);
    });

    public override void OnEnvControl(byte index, uint state)
    {
        bool shouldSet = curState == State.Ready;
        if (shouldSet)
        {
            bool validIndex = (uint)(index - 14) <= 1u;
            shouldSet = validIndex;
        }
        if (shouldSet)
        {
            SetState((index == 14) ? State.NextNS : State.NextEW);
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        switch (info.ActionId)
        {
        case 37883u:
            curState = State.Ready;
            break;
        case 38542u:
            SetState(State.NextCorners);
            break;
        case 37884u:
            SetState(State.NextCenter);
            break;
        case 38543u:
            SetState(State.Done);
            break;
        }
    }

    private void SetState(State state)
    {
        if (curState != state)
        {
            curState = state;
            switch (state)
            {
            case State.NextNS:
            {
                DrawElement element = new DrawElement
                {
                    Enable = false,
                    drawAvfx = "e5d1_b1_kblaser_t1",
                    radiusX = 1f,
                    radiusZ = 22f,
                    KnockBackCheck = new KnockBackCheck
                    {
                        OriginPos = new Vector3(100f, 0f, 89f)
                    },
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 38542u }
                    }
                };
                aoes.Add(DrawManager.Draw(element, (IGameObject?)Svc.Objects.LocalPlayer));
                element.KnockBackCheck = new KnockBackCheck
                {
                    OriginPos = new Vector3(100f, 0f, 111f)
                };
                aoes.Add(DrawManager.Draw(element, (IGameObject?)Svc.Objects.LocalPlayer));
                break;
            }
            case State.NextEW:
            {
                DrawElement element = new DrawElement
                {
                    Enable = false,
                    drawAvfx = "e5d1_b1_kblaser_t1",
                    radiusX = 1f,
                    radiusZ = 22f,
                    KnockBackCheck = new KnockBackCheck
                    {
                        OriginPos = new Vector3(89f, 0f, 100f)
                    },
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 38542u }
                    }
                };
                aoes.Add(DrawManager.Draw(element, (IGameObject?)Svc.Objects.LocalPlayer));
                element.KnockBackCheck = new KnockBackCheck
                {
                    OriginPos = new Vector3(111f, 0f, 100f)
                };
                aoes.Add(DrawManager.Draw(element, (IGameObject?)Svc.Objects.LocalPlayer));
                break;
            }
            case State.NextCorners:
            {
                DrawElement element = new DrawElement
                {
                    Enable = false,
                    drawAvfx = "e5d1_b1_kblaser_t1",
                    radiusX = 1f,
                    radiusZ = 18f,
                    KnockBackCheck = new KnockBackCheck
                    {
                        OriginPos = new Vector3(89f, 0f, 89f)
                    },
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 37884u }
                    }
                };
                aoes.Add(DrawManager.Draw(element, (IGameObject?)Svc.Objects.LocalPlayer));
                element.KnockBackCheck = new KnockBackCheck
                {
                    OriginPos = new Vector3(89f, 0f, 111f)
                };
                aoes.Add(DrawManager.Draw(element, (IGameObject?)Svc.Objects.LocalPlayer));
                element.KnockBackCheck = new KnockBackCheck
                {
                    OriginPos = new Vector3(111f, 0f, 89f)
                };
                aoes.Add(DrawManager.Draw(element, (IGameObject?)Svc.Objects.LocalPlayer));
                element.KnockBackCheck = new KnockBackCheck
                {
                    OriginPos = new Vector3(111f, 0f, 111f)
                };
                aoes.Add(DrawManager.Draw(element, (IGameObject?)Svc.Objects.LocalPlayer));
                break;
            }
            case State.NextCenter:
            {
                DrawElement element = new DrawElement
                {
                    Enable = false,
                    drawAvfx = "e5d1_b1_kblaser_t1",
                    radiusX = 1f,
                    radiusZ = 14f,
                    KnockBackCheck = new KnockBackCheck
                    {
                        OriginPos = new Vector3(100f, 0f, 100f)
                    },
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 38543u }
                    }
                };
                aoes.Add(DrawManager.Draw(element, (IGameObject?)Svc.Objects.LocalPlayer));
                break;
            }
            }
        }
    }

    public override void Reset()
    {
        curState = State.None;
        base.Reset();
    }
}
