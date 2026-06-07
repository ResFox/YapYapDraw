using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M4S;

public class SunriseSabbath : ISpecialAction
{
    public override string Name => "Sunrise Sabbath (bait)";

    public override uint Phase => 8u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38437u, 38438u, 39257u, 39258u };

    public override IEnumerable<StaticVfx> ActiveAOEs
    {
        get
        {
            if (aoes.Count == 0 || Svc.Objects.LocalPlayer == null)
            {
                return Array.Empty<StaticVfx>();
            }
            List<StaticVfx> result = new List<StaticVfx>();
            bool positron = ((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(4000u);
            bool negatron = ((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(4001u);
            if ((positron || negatron) && (((IGameObject)Svc.Objects.LocalPlayer).GetStatusRemainingTime(4000u, out var time) || ((IGameObject)Svc.Objects.LocalPlayer).GetStatusRemainingTime(4001u, out time)) && time < 15f)
            {
                if (Vector3.Distance(((IGameObject)Svc.Objects.LocalPlayer).Position, new Vector3(100f, 0f, 165f)) < 12f)
                {
                    if (positron)
                    {
                        foreach (StaticVfx aoe in aoes)
                        {
                            if (aoe.Owner.GetParam(2970u, out var param) && param == 757)
                            {
                                result.Add(aoe);
                            }
                        }
                    }
                    else if (negatron)
                    {
                        foreach (StaticVfx aoe2 in aoes)
                        {
                            if (aoe2.Owner.GetParam(2970u, out var param2) && param2 == 756)
                            {
                                result.Add(aoe2);
                            }
                        }
                    }
                }
                else
                {
                    foreach (StaticVfx nearest in aoes.OrderBy((StaticVfx aoe) => Vector3.Distance(((IGameObject)Svc.Objects.LocalPlayer).Position, aoe.Owner.Position)).Take(2))
                    {
                        result.Add(nearest);
                    }
                }
            }
            return result;
        }
    }

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        bool isCluster = info.StatusID == 2970;
        if (isCluster)
        {
            uint stack = info.Stack;
            bool relevantStack = stack - 756 <= 1;
            isCluster = relevantStack;
        }
        if (isCluster)
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = ((info.Stack == 756) ? "general02xf" : "general02pxf"),
                radiusX = 6f,
                radiusZ = 40f,
                drawOnObject = true,
                fixRotation = false,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38437u, 38438u, 39257u, 39258u }
                }
            };
            aoes.Add(DrawManager.Draw(element, info.TargetID.GameObject()));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
