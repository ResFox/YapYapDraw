using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.ShishuVc;

public class MermaidDariaHarmonicSerenade : ISpecialAction
{
    private List<(uint BaseId, DrawElement Element)> timeline = new List<(uint, DrawElement)>();

    private List<(uint BaseId, DrawElement Element)> timelineHistory = new List<(uint, DrawElement)>();

    private int index;

    private uint lastAbilityId;

    public override string Name => "Mermaid Daria Harmonic Serenade";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45771u, 45773u, 45844u, 45839u, 45840u, 45841u, 45842u, 45843u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 45771)
        {
            Reset();
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        bool isTimelineAction = actionId == 45773 || actionId - 45839 <= 5;
        if (!isTimelineAction || info.ActionId == lastAbilityId)
        {
            return;
        }
        lastAbilityId = info.ActionId;
        switch (info.ActionId)
        {
        case 45773u:
            timelineHistory = timeline.ToList();
            break;
        case 45844u:
            timeline = timelineHistory.ToList();
            break;
        }
        Plugin.DebugLog($"timeline:{timeline.Count}, index:{index}, timelineHistory:{timelineHistory.Count}");
        if (index >= timeline.Count)
        {
            return;
        }
        (uint, DrawElement) entry = timeline[index++];
        foreach (IGameObject actor in (IEnumerable<IGameObject>)Svc.Objects)
        {
            if (actor.BaseId == entry.Item1)
            {
                DrawManager.Draw(entry.Item2, actor);
            }
        }
    }

    public override void OnActorTargetVfx(uint actorId, uint targetVfxId)
    {
        IGameObject actor = actorId.GameObject();
        if (actor != null && actor.BaseId == 19097)
        {
            switch (targetVfxId)
            {
            case 2746u:
                timeline.Add((19102u, CreateFan(45843u)));
                break;
            case 2744u:
                timeline.Add((19100u, CreateRect(45841u)));
                break;
            case 2743u:
                timeline.Add((19099u, CreateRect(45840u)));
                break;
            case 2741u:
                timeline.Add((19098u, CreateRect(45839u)));
                break;
            case 2745u:
                timeline.Add((19101u, CreateCircle(45842u)));
                break;
            case 2742u:
                break;
            }
        }
    }

    private static DrawElement CreateFan(uint actionId)
    {
        return new DrawElement
        {
            drawAvfx = "gl_fan060_1bf",
            radiusX = 45f,
            radiusZ = 45f,
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { actionId }
            }
        };
    }

    private static DrawElement CreateRect(uint actionId)
    {
        return new DrawElement
        {
            drawAvfx = "general02xf",
            radiusX = 4f,
            radiusZ = 40f,
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { actionId }
            }
        };
    }

    private static DrawElement CreateCircle(uint actionId)
    {
        return new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 20f,
            radiusZ = 20f,
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { actionId }
            }
        };
    }

    public override void Reset()
    {
        timeline.Clear();
        index = 0;
        lastAbilityId = 0u;
        base.Reset();
    }
}
