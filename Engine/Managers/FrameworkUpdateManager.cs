using System.Collections.Generic;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Struct.Vfx;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Engine.Managers;

public static class FrameworkUpdateManager
{
    public static List<TimeHelper> TimeHelpers = new();

    public static List<StaticVfx> StaticVfxs = new();

    public static List<ActorVfx> ActorVfxs = new();

    public static void Tick()
    {
        if (Svc.Objects.LocalPlayer == null)
        {
            return;
        }

        FightClientState.PollEnmity();
        VFXList.SyncVfxHandles();

        TimeHelper[] timers = TimeHelpers.ToArray();
        for (int i = 0; i < timers.Length; i++)
        {
            timers[i].Update();
        }

        ActorVfx[] actorVfxs = ActorVfxs.ToArray();
        for (int i = 0; i < actorVfxs.Length; i++)
        {
            actorVfxs[i].Update();
        }

        StaticVfx[] staticVfxs = StaticVfxs.ToArray();
        for (int i = 0; i < staticVfxs.Length; i++)
        {
            staticVfxs[i].Update();
        }
    }
}
