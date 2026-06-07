using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.DancingMad.P2;

public class AberrantTriangle : ISpecialAction
{
    public override string Name => "Aberrant Triangle";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47840u };

    public override IEnumerable<StaticVfx> ActiveAOEs
    {
        get
        {
            if (aoes.Count == 0)
                return Array.Empty<StaticVfx>();

            long initTime = aoes[0].initTime;
            int i;
            for (i = 0; i < aoes.Count && aoes[i].initTime - initTime < 1000; i++)
            {
            }
            return aoes.Slice(0, i);
        }
    }

    public override void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
    {
        IGameObject? eventObj = actorID.GameObject();
        if (eventObj == null || eventObj.BaseId - 2015154 > 1 || p1 != 16 || p2 != 32)
            return;

        Vector3 anchor;
        if (eventObj.BaseId == 2015154)
        {
            anchor = new Vector3(eventObj.Position.X - 3f, eventObj.Position.Y, eventObj.Position.Z + 5f);
            if (eventObj.Position.AlmostEqual(new Vector3(88.453f, 0f, 90f), 1f))
                anchor = new Vector3(eventObj.Position.X + 3f, eventObj.Position.Y, eventObj.Position.Z + 5f);
        }
        else
        {
            anchor = new Vector3(eventObj.Position.X + 3f, eventObj.Position.Y, eventObj.Position.Z + 5f);
        }

        Vector2 baseOffset = (anchor - eventObj.Position).ToVector2();
        for (int i = 0; i < 3; i++)
        {
            Vector2 rot = baseOffset.RotationDegress(i * 120f);
            Vector3 position = eventObj.Position + new Vector3(rot.X, 0f, rot.Y);
            DrawElement element = new DrawElement
            {
                drawAvfx = "m0347_sircle_01m1",
                Position = position,
                drawOnObject = false,
                radiusX = 6f,
                radiusZ = 6f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47840u },
                    TargetHitCount = 21
                }
            };
            aoes.Add(DrawManager.Draw(element));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
