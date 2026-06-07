using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M12S.Body;

public class MimicCellSecond : ISpecialAction
{
    public enum LineType
    {
        JumpLine,
        ShareLine,
        FanLine,
        CircleLine,
        NoneLine
    }

    public Queue<IGameObject> mimicSpawnOrder = new Queue<IGameObject>();

    public Dictionary<IGameObject, IGameObject> mimicCellPairs = new Dictionary<IGameObject, IGameObject>();

    public Dictionary<IGameObject, (ulong, LineType)> playerTetherMap = new Dictionary<IGameObject, (ulong, LineType)>();

    public List<IGameObject> LockLineList = new List<IGameObject>();

    public bool Enable;

    public override string Name => "Mimic Cell (Second Fate)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46307u, 46316u, 46382u, 46380u, 46319u, 47394u, 48099u };

    public override uint Phase => 2u;

    public override void Reset()
    {
        mimicSpawnOrder.Clear();
        mimicCellPairs.Clear();
        playerTetherMap.Clear();
        LockLineList.Clear();
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 46307)
        {
            foreach (KeyValuePair<IGameObject, (ulong, LineType)> pair in playerTetherMap)
            {
                if (!LockLineList.Contains(pair.Key))
                {
                    playerTetherMap[pair.Key] = (0uL, LineType.NoneLine);
                }
            }
            foreach (KeyValuePair<IGameObject, (ulong, LineType)> pair in playerTetherMap)
            {
                DrawElement element = new DrawElement();
                switch (pair.Value.Item2)
                {
                case LineType.NoneLine:
                    element = new DrawElement
                    {
                        drawAvfx = "general_1bxf",
                        drawOnObject = true,
                        drawType = ElementType.Omen,
                        radiusX = 20f,
                        radiusZ = 20f,
                        hitCounter = new HitCounter
                        {
                            ActionID = new HashSet<uint> { 46311u }
                        }
                    };
                    break;
                case LineType.ShareLine:
                    element = new DrawElement
                    {
                        drawAvfx = "com_share_4_5s_c0c",
                        drawOnObject = true,
                        drawType = ElementType.LockOn,
                        delayDrawTime = info.CastTime * 1000f + 3000f,
                        hitCounter = new HitCounter
                        {
                            ActionID = new HashSet<uint> { 46319u }
                        }
                    };
                    DrawManager.Draw(element, pair.Key);
                    break;
                case LineType.FanLine:
                    element = new DrawElement
                    {
                        drawAvfx = "gl_fan030_1bf",
                        drawOnObject = true,
                        drawType = ElementType.Omen,
                        radiusX = 50f,
                        radiusZ = 50f,
                        delayDrawTime = info.CastTime * 1000f + 3000f,
                        hitCounter = new HitCounter
                        {
                            ActionID = new HashSet<uint> { 46315u }
                        }
                    };
                    break;
                case LineType.CircleLine:
                    element = new DrawElement
                    {
                        drawAvfx = "general_1bxf",
                        drawOnObject = true,
                        drawType = ElementType.Omen,
                        radiusX = 20f,
                        radiusZ = 20f,
                        delayDrawTime = info.CastTime * 1000f - 2000f,
                        hitCounter = new HitCounter
                        {
                            ActionID = new HashSet<uint> { 46311u }
                        }
                    };
                    break;
                }
                DrawManager.Draw(element, pair.Key);
            }
        }
        if (info.ActionId == 46316)
        {
            DrawAoe();
            DrawAoe();
        }
        if (info.ActionId == 46382)
        {
            foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "share2_6m",
                    radiusX = 6f,
                    radiusY = 6f,
                    radiusZ = 6f,
                    drawOnObject = true,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 46302u }
                    },
                    distanceCheck = new DistanceCheck
                    {
                        CheckType = 2,
                        Count = 2,
                        CheckObject = info.SourceId.GameObject()
                    }
                }, allPlayer);
            }
        }
        if (info.ActionId != 46380)
        {
            return;
        }
        foreach (IGameObject allPlayer2 in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "share2_6m",
                radiusX = 6f,
                radiusY = 6f,
                radiusZ = 6f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 46381u }
                },
                distanceCheck = new DistanceCheck
                {
                    CheckType = 3,
                    Count = 1,
                    CheckObject = info.SourceId.GameObject()
                }
            }, allPlayer2);
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        bool shouldDraw;
        switch (info.ActionId)
        {
        case 46319u:
        case 46380u:
        case 46382u:
        case 47394u:
        case 48099u:
            shouldDraw = true;
            break;
        default:
            shouldDraw = false;
            break;
        }
        if (shouldDraw)
        {
            DrawAoe();
        }
    }

    public void DrawAoe()
    {
        if (mimicSpawnOrder.Count <= 0)
        {
            return;
        }
        IGameObject mimic = mimicSpawnOrder.Dequeue();
        DrawElement element = new DrawElement();
        try
        {
            switch (playerTetherMap[mimicCellPairs[mimic]].Item2)
            {
            case LineType.NoneLine:
                element = new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    drawOnObject = true,
                    drawType = ElementType.Omen,
                    radiusX = 20f,
                    radiusZ = 20f,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 48099u }
                    }
                };
                DrawManager.Draw(element, mimic);
                break;
            case LineType.ShareLine:
                element = new DrawElement
                {
                    drawAvfx = "com_share_4_5s_c0c",
                    drawOnObject = true,
                    drawType = ElementType.LockOn,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 46319u }
                    }
                };
                DrawManager.Draw(element, mimic);
                break;
            case LineType.FanLine:
                element = new DrawElement
                {
                    drawAvfx = "gl_fan045_1bf",
                    drawOnObject = true,
                    drawType = ElementType.Omen,
                    radiusX = 50f,
                    radiusZ = 50f,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 47394u }
                    }
                };
                break;
            case LineType.CircleLine:
                element = new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    drawOnObject = true,
                    drawType = ElementType.Omen,
                    radiusX = 20f,
                    radiusZ = 20f,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 48099u }
                    }
                };
                break;
            }
            DrawManager.Draw(element, mimic);
        }
        catch
        {
        }
    }

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (source != null && source.BaseId == 19210 && id == 4562)
        {
            mimicSpawnOrder.Enqueue(source);
        }
    }

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (actorId.GameObject().BaseId == 19210 && Id == 373)
        {
            mimicCellPairs.Add(actorId.GameObject(), targetId.GameObject());
        }
        uint baseId = actorId.GameObject().BaseId;
        if (baseId == 19202 || baseId == 19204)
        {
            switch (Id)
            {
            case 367u:
                playerTetherMap[targetId.GameObject()] = (actorId, LineType.FanLine);
                break;
            case 368u:
                playerTetherMap[targetId.GameObject()] = (actorId, LineType.CircleLine);
                break;
            case 369u:
                playerTetherMap[targetId.GameObject()] = (actorId, LineType.ShareLine);
                break;
            case 374u:
                playerTetherMap[targetId.GameObject()] = (actorId, LineType.JumpLine);
                break;
            case 373u:
                LockLineList.Add(targetId.GameObject());
                break;
            case 370u:
            case 371u:
            case 372u:
                break;
            }
        }
    }
}
