using System;
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
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.ShishuVc;

public class FairyPellyIceFireGleam : ISpecialAction
{
    private IGameObject? ice;

    public override string Name => "Fairy Pelly Ice/Fire Gleam";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 45548u, 45504u, 45506u };

    public override IEnumerable<StaticVfx> ActiveAOEs
    {
        get
        {
            int count = aoes.Count;
            if (count == 0)
            {
                return Array.Empty<StaticVfx>();
            }
            long initTime = aoes[0].initTime;
            int i;
            for (i = 0; i < count && aoes[i].initTime - initTime < 1000; i++)
            {
            }
            return aoes.Slice(0, i);
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 45548:
        {
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "general_x02f",
                Position = info.Pos,
                radiusX = 5f,
                radiusZ = 40f,
                drawOnObject = false,
                destroyTime = info.CastTime * 1000f
            };
            aoes.Add(DrawManager.Draw(drawElement));
            drawElement.refRotation += 90.Degrees();
            aoes.Add(DrawManager.Draw(drawElement));
            break;
        }
        case 45504:
        {
            IGameObject anchor = Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 19059);
            if (info.Pos.AlmostEqual(anchor.Position, 1f))
            {
                ice = info.SourceId.GameObject();
            }
            break;
        }
        case 45506:
            if (ice != null && info.SourceId == ice.GameObjectId)
            {
                DrawElement drawElement = new DrawElement
                {
                    drawAvfx = "m0973_lzr_ice_o0e1",
                    Position = ice.Position,
                    radiusX = 5f,
                    radiusZ = 40f,
                    drawOnObject = false,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 45501u }
                    }
                };
                DrawManager.Draw(drawElement);
                drawElement.refRotation += 90.Degrees();
                DrawManager.Draw(drawElement);
            }
            break;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 45548 && aoes.Count > 1)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }

    public override void Reset()
    {
        ice = null;
        base.Reset();
    }
}
