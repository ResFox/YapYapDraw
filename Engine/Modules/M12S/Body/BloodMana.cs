using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M12S.Body;

public class BloodMana : ISpecialAction
{
    private static readonly uint[] OrbBaseIds = { 19206u, 19207u, 19208u, 19209u };
    private static readonly Vector2 EastAnchor = new(110f, 100f);
    private static readonly Vector2 WestAnchor = new(90f, 100f);
    private const float PlatformRadius = 10f;

    private readonly Dictionary<ulong, StaticVfx> _marks = new();

    private long _showAfterMs;
    private long _showUntilMs;
    private bool _captured;

    public override string Name => "Blood Mana";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 46333u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId != 46333u)
        {
            return;
        }

        ClearMarks();
        long now = Environment.TickCount64;
        _showAfterMs = now + 1000;
        _showUntilMs = now + 11000;
        _captured = false;
    }

    public override void Update()
    {
        if (_showUntilMs == 0)
        {
            return;
        }

        long now = Environment.TickCount64;
        if (now >= _showUntilMs)
        {
            ClearMarks();
            return;
        }

        if (now < _showAfterMs || _captured)
        {
            return;
        }

        _captured = true;

        Dictionary<uint, List<IGameObject>> byType = new Dictionary<uint, List<IGameObject>>();
        foreach (IGameObject obj in Svc.Objects)
        {
            if (!IsOrb(obj) || !IsVisible(obj))
            {
                continue;
            }

            if (!byType.TryGetValue(obj.BaseId, out List<IGameObject>? list))
            {
                list = new List<IGameObject>();
                byType[obj.BaseId] = list;
            }

            list.Add(obj);
        }

        HashSet<ulong> wanted = new HashSet<ulong>();
        foreach (List<IGameObject> orbs in byType.Values)
        {
            CollectFarOrbs(orbs, EastAnchor, wanted);
            CollectFarOrbs(orbs, WestAnchor, wanted);
        }

        foreach (ulong id in _marks.Keys)
        {
            if (wanted.Contains(id))
            {
                continue;
            }

            RemoveMark(id);
        }

        foreach (ulong id in wanted)
        {
            if (_marks.ContainsKey(id))
            {
                continue;
            }

            IGameObject? obj = id.GameObject();
            if (obj == null)
            {
                continue;
            }

            StaticVfx? vfx = DrawManager.Draw(new DrawElement
            {
                drawAvfx = "customCircle",
                radiusX = 4f,
                radiusZ = 4f,
                drawOnObject = true,
                refColor = PickGreen,
                refTargetColor = PickGreen,
                destroyTime = 600000f
            }, obj);

            if (vfx != null)
            {
                _marks[id] = vfx;
                aoes.Add(vfx);
            }
        }
    }

    public override void Reset()
    {
        ClearMarks();
        base.Reset();
    }

    private static Vector4 PickGreen => new Vector4(0.1f, 1f, 0.1f, YapYapDraw.Plugin.Config.CustomAlpha);

    private static bool IsOrb(IGameObject obj)
    {
        uint id = obj.BaseId;
        foreach (uint orbId in OrbBaseIds)
        {
            if (id == orbId)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsVisible(IGameObject obj)
    {
        if (obj is ICharacter character)
        {
            return character.IsCharacterVisible();
        }

        return true;
    }

    private static void CollectFarOrbs(List<IGameObject> sameType, Vector2 anchor, HashSet<ulong> wanted)
    {
        bool closeExists = false;
        foreach (IGameObject orb in sameType)
        {
            if (DistanceTo(orb, anchor) <= PlatformRadius)
            {
                closeExists = true;
                break;
            }
        }

        if (!closeExists)
        {
            return;
        }

        foreach (IGameObject orb in sameType)
        {
            if (DistanceTo(orb, anchor) > PlatformRadius)
            {
                wanted.Add(orb.GameObjectId);
            }
        }
    }

    private static float DistanceTo(IGameObject orb, Vector2 anchor)
    {
        return Vector2.Distance(new Vector2(orb.Position.X, orb.Position.Z), anchor);
    }

    private void RemoveMark(ulong id)
    {
        if (!_marks.TryGetValue(id, out StaticVfx? vfx))
        {
            return;
        }

        vfx?.Remove();
        aoes.Remove(vfx);
        _marks.Remove(id);
    }

    private void ClearMarks()
    {
        foreach (StaticVfx vfx in _marks.Values)
        {
            vfx?.Remove();
        }

        _marks.Clear();
        aoes.Clear();
        _showAfterMs = 0;
        _showUntilMs = 0;
        _captured = false;
    }
}
