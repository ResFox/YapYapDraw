using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M5S;

public class FlipToABSide : ISpecialAction
{
    public bool isBside;

    public uint sourceId;

    public override string Name => "Flip To A/B-Side";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42880u, 42881u, 42798u, 42807u, 42817u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 42881)
        {
            sourceId = info.SourceId;
            isBside = true;
        }
        else if (info.ActionId == 42880)
        {
            sourceId = info.SourceId;
            isBside = false;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        if ((actionId != 42798 && actionId != 42807 && actionId != 42817) || 1 == 0)
        {
            return;
        }
        if (!isBside)
        {
            IGameObject[] targets = (IGameObject[])new IGameObject[3]
            {
                PlayerHelper.Tank.FirstOrDefault(),
                PlayerHelper.Healer.FirstOrDefault(),
                PlayerHelper.DPS.FirstOrDefault()
            };
            foreach (IGameObject target in targets)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "gl_fan045_1bpxf",
                    radiusX = 40f,
                    radiusZ = 40f,
                    drawOnObject = true,
                    target = target,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 42883u }
                    }
                }, sourceId.GameObject());
            }
            return;
        }
        foreach (IGameObject healer in PlayerHelper.Healer)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02pxf",
                radiusX = 4f,
                radiusZ = 50f,
                drawOnObject = true,
                target = healer,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 42884u }
                }
            }, sourceId.GameObject());
            SimpleLockon.ShareRect4s(healer, sourceId.GameObject());
        }
    }
}
