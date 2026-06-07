using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M1S;

public class ElevateAndEviscerate : ISpecialAction
{
    private IGameObject? iconTarget;

    public override string Name => "Elevate and Eviscerate";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37958u, 37960u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (iconTarget == Svc.Objects.LocalPlayer)
        {
            switch (info.ActionId)
            {
            case 37958:
                SimpleElement.ShowText("Knockup", (TextGimmickHintStyle)0);
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "e5d1_b1_kblaser_t1",
                    radiusX = 1f,
                    radiusZ = 10f,
                    drawOnObject = true,
                    refColor = new Vector4(1f, 1f, 1f, 3f),
                    refTargetColor = new Vector4(1f, 1f, 1f, 3f),
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 37962u }
                    }
                }, iconTarget);
                break;
            case 37960:
                SimpleElement.ShowText("Slam down", (TextGimmickHintStyle)0);
                break;
            }
        }
    }

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon != 538)
        {
            return;
        }
        iconTarget = target;
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_x02f",
            radiusX = 5f,
            radiusZ = 60f,
            drawOnObject = false,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 37962u }
            },
            PositionCustomAction = () =>
            {
                Vector3 result = new Vector3(0f, 0f, 0f);
                (int, int, int)[] bands = new(int, int, int)[4]
                {
                    (int.MinValue, 90, 85),
                    (90, 100, 95),
                    (100, 110, 105),
                    (110, int.MaxValue, 115)
                };
                (int, int, int)[] scan = bands;
                for (int i = 0; i < scan.Length; i++)
                {
                    (int, int, int) band = scan[i];
                    if (target.Position.X >= (float)band.Item1 && target.Position.X < (float)band.Item2)
                    {
                        result.X = band.Item3;
                        break;
                    }
                }
                scan = bands;
                for (int i = 0; i < scan.Length; i++)
                {
                    (int, int, int) band = scan[i];
                    if (target.Position.Z >= (float)band.Item1 && target.Position.Z < (float)band.Item2)
                    {
                        result.Z = band.Item3;
                        break;
                    }
                }
                return result;
            }
        }, target);
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_x02f",
            radiusX = 5f,
            radiusZ = 60f,
            drawOnObject = false,
            refRotation = 90.Degrees(),
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 37962u }
            },
            PositionCustomAction = () =>
            {
                Vector3 result = new Vector3(0f, 0f, 0f);
                (int, int, int)[] bands = new(int, int, int)[4]
                {
                    (int.MinValue, 90, 85),
                    (90, 100, 95),
                    (100, 110, 105),
                    (110, int.MaxValue, 115)
                };
                (int, int, int)[] scan = bands;
                for (int i = 0; i < scan.Length; i++)
                {
                    (int, int, int) band = scan[i];
                    if (target.Position.X >= (float)band.Item1 && target.Position.X < (float)band.Item2)
                    {
                        result.X = band.Item3;
                        break;
                    }
                }
                scan = bands;
                for (int i = 0; i < scan.Length; i++)
                {
                    (int, int, int) band = scan[i];
                    if (target.Position.Z >= (float)band.Item1 && target.Position.Z < (float)band.Item2)
                    {
                        result.Z = band.Item3;
                        break;
                    }
                }
                return result;
            }
        }, target);
    }
}
