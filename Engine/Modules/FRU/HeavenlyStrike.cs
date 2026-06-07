using System;
using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class HeavenlyStrike : ISpecialAction
{
    private readonly List<Vector3> pos = new List<Vector3>();

    public override string Name => "Heavenly Strike (knockback)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40198u, 40208u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 40198 && pos.Count < 2)
        {
            pos.Add(info.Pos);
        }
        if (info.ActionId != 40208)
        {
            return;
        }
        foreach (Vector3 po in pos)
        {
            Vector3 offset = po - new Vector3(100f, 0f, 100f);
            Angle refRotation = new Angle(MathF.Atan2(offset.X, offset.Z));
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "e5d1_b1_kblaser_t1",
                Position = new Vector3(100f, 0f, 100f),
                drawOnObject = false,
                radiusX = 2f,
                radiusZ = 18f,
                refRotation = refRotation,
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40207u }
                }
            });
        }
        pos.Clear();
    }

    public override void Reset()
    {
        pos.Clear();
        base.Reset();
    }
}
