using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Engine.Preview;

// Spawns the UMAD telegraphs one at a time around the player so they can be
// eyeballed without the duty. Same omens/shapes the modules use; no HitCounter,
// so each clears on its own timer.
public static class UmadPreview
{
    private const float StepMs = 3500f;

    public static bool Run()
    {
        IGameObject? lp = Svc.Objects.LocalPlayer;
        if (lp == null)
            return false;

        Vector3 origin = lp.Position;
        Angle facing = lp.Rotation.Radians();
        int slot = 0;

        void Omen(string avfx, float rx, float rz, bool fix = false, Vector4? color = null, Vector3? pos = null)
        {
            var e = new DrawElement
            {
                drawAvfx = avfx,
                Position = pos ?? origin,
                drawOnObject = false,
                radiusX = rx,
                radiusZ = rz,
                refRotation = facing,
                fixRotation = fix,
                destroyTime = StepMs,
                delayDrawTime = slot * StepMs,
            };
            if (color.HasValue)
            {
                e.refColor = color.Value;
                e.refTargetColor = color.Value;
            }
            DrawManager.Draw(e);
        }

        float Delay() => slot * StepMs;

        // P1
        SimpleElement.Fan(origin, 100f, 120, facing, StepMs, Delay());              // Ferocious Laceration
        slot++;
        SimpleElement.Rectangle(origin, 100f, 3f, 0f, facing, StepMs, Delay());     // Wave Cannon
        slot++;
        SimpleElement.Fan(origin, 40f, 90, facing, StepMs, Delay());                // Wondrous Magic (fan)
        SimpleElement.Rectangle(origin, 40f, 5f, 0f, facing, StepMs, Delay());      // Wondrous Magic (line)
        slot++;
        SimpleElement.Fan(origin, 100f, 180, facing, StepMs, Delay());             // Idol Half-Room Cleave
        slot++;
        DrawManager.Draw(new DrawElement                                            // Idol Face/Away (channeling)
        {
            drawType = ElementType.Channeling,
            drawAvfx = "chn_miro1v",
            destroyTime = StepMs,
            delayDrawTime = Delay(),
        }, lp, lp);
        slot++;
        SimpleElement.Circle(origin, 5f, StepMs, Delay());                          // Overdrive
        slot++;
        Omen("nockback_omen04t1", 6f, 6f);                                          // Chain Trap
        slot++;
        Omen(GroundOmen.ArrowRect, 1f, 10f, fix: true);                            // Idol Tethers (laser)
        slot++;

        // P2
        Omen(GroundOmen.CircleFull, 6f, 6f);                                        // Aberrant Triangle
        Omen(GroundOmen.CircleFull, 6f, 6f, pos: origin + Offset(facing, 8f, 120f));
        Omen(GroundOmen.CircleFull, 6f, 6f, pos: origin + Offset(facing, 8f, 240f));
        slot++;
        SimpleElement.Fan(origin, 100f, 180, facing, StepMs, Delay());             // Annihilation Kick
        slot++;
        Omen(GroundOmen.RectangleFull, 20f, 80f, fix: true);                       // Destruction Wing (wing)
        Omen("tank_lockon_5m_5s_noc", 7f, 7f, color: GroundOmen.Red);             // Destruction Wing (tank)
        slot++;
        SimpleElement.Circle(origin, 5f, StepMs, Delay());                          // Endbound Bracelets
        slot++;
        SimpleElement.Circle(origin, 5f, StepMs, Delay());                          // Past/Future (player circle)
        Omen(GroundOmen.Fan180, 100f, 100f, fix: true);                           // Past/Future (fan)
        slot++;
        Omen(GroundOmen.Fan090, 40f, 40f, fix: true);                             // Forsaken Eschaton (fan)
        Omen(GroundOmen.Friendly.Circle, 5f, 5f, pos: origin + Offset(facing, 8f, 0f)); // spread tower
        slot++;

        SimpleElement.ShowText("UMAD telegraph preview");
        return true;
    }

    private static Vector3 Offset(Angle facing, float dist, float degrees)
    {
        Angle a = facing + degrees.Degrees();
        return new Vector3(dist * (float)System.Math.Sin(a.Rad), 0f, dist * (float)System.Math.Cos(a.Rad));
    }
}
