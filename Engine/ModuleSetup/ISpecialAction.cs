using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;

using YapYapDraw.Engine.Host;

namespace YapYapDraw.Engine.ModuleSetup;

public abstract class ISpecialAction
{
    public abstract string Name { get; }

    public virtual string GUID { get; protected set; } = string.Empty;

    public virtual bool HasConfig => false;

    /// Set false to exclude a mechanic from discovery (kept in source, not loaded).
    public virtual bool Registered => true;

    /// fightKey/mechanicName — used for persisted on/off state in plugin config.
    public virtual string? ModuleEnableKey => null;

    public abstract HashSet<uint> ActionID { get; }

    public virtual uint WeatherID { get; }

    public virtual uint Phase { get; } = 1u;

    public virtual Queue<(HashSet<uint>, (IGameObject?, DrawElement[])[])> DrawQueue { get; protected set; } = new Queue<(HashSet<uint>, (IGameObject, DrawElement[])[])>();

    public virtual Queue<(HashSet<uint>, Action)> ActionQueue { get; protected set; } = new Queue<(HashSet<uint>, Action)>();

    public virtual List<StaticVfx> aoes { get; protected set; } = new List<StaticVfx>();

    public virtual IEnumerable<StaticVfx> ActiveAOEs => Array.Empty<StaticVfx>();

    public int NumCasts { get; protected set; }

    public bool CanDraw { get; protected set; }

    public virtual void Setup()
    {
        GUID = ModuleLoader.Sha256Hex($"{GetType().FullName}-{Phase}-{Name}");
    }

    public virtual void DrawConfig()
    {
    }

    public virtual void Reset()
    {
        DrawQueue.Clear();
        ActionQueue.Clear();
        NumCasts = 0;
        CanDraw = false;
        foreach (StaticVfx aoe in aoes)
        {
            aoe?.Remove();
        }
        aoes.Clear();
    }

    public virtual void Update()
    {
    }

    public virtual void OnActionCast(ActorCastInfo info)
    {
    }

    public virtual void OnAbilityCast(ActorAbilityInfo info)
    {
    }

    public void OnDrawQueue(ActorAbilityInfo info)
    {
        if (DrawQueue.Count > 0)
        {
            var (actionIds, draws) = DrawQueue.Peek();
            if (actionIds.Contains(info.ActionId))
            {
                foreach (var (target, elements) in draws)
                {
                    foreach (var element in elements)
                    {
                        DrawManager.Draw(element, target);
                    }
                }
                DrawQueue.Dequeue();
            }
        }
        if (ActionQueue.Count > 0)
        {
            var (actionIds, action) = ActionQueue.Peek();
            if (actionIds.Contains(info.ActionId))
            {
                action();
                ActionQueue.Dequeue();
            }
        }
    }

    public virtual void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
    }

    public virtual void OnChatMessage(uint chatType, string content)
    {
    }

    public virtual void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
    }

    public virtual void OnActorTetherCancelEvent(uint actorID)
    {
    }

    public virtual void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
    }

    public virtual void OnHeaderMarkerChangeEvent(HeaderMarkerEnum headerMarker)
    {
    }

    public virtual void OnObjectCreatedEvent(IGameObject GameObject)
    {
    }

    public virtual void OnMapEffect(uint a2, ushort a3, ushort a4)
    {
    }

    public virtual void OnEnvControl(byte index, uint state)
    {
    }

    public virtual void OnAddStatus(ActorStatusChangeInfo info)
    {
    }

    public virtual void OnRemoveStatus(ActorStatusChangeInfo info)
    {
    }

    public virtual void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
    {
    }

    public virtual void OnNpcYell(ulong SourceID, ushort Message)
    {
    }

    public virtual void OnWeatherChange(uint oldWeatherID, uint newWeatherID)
    {
    }

    public virtual void OnDutyStartRecommenced()
    {
    }

    public virtual void OnActorModelStateChange(IGameObject obj, byte modelState, byte animState1, byte animState2)
    {
    }

    public virtual void OnActorTargetVfx(uint actorId, uint targetVfxId)
    {
    }

    public virtual void OnActorControl(uint sourceId, uint command, uint p1, uint p2, uint p3, uint p4)
    {
    }
}
