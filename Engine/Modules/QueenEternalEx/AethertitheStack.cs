using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.QueenEternalEx;
public class AethertitheStack : ISpecialAction
{
    public override string Name => "Aethertithe (stack)";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnEnvControl(byte index, uint state)
    {
        if (index != 0)
        {
            return;
        }
        IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18039);
        if ((state != 67109120 && state != 134217984 && state != 268435712) || 1 == 0)
        {
            return;
        }
        foreach (IGameObject item in PlayerHelper.Healer)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02pxf",
                radiusX = 4f,
                radiusZ = 60f,
                target = item,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40978u }
                }
            }, target);
            SimpleLockon.ShareRect8s(item);
        }
    }
}
