using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class HolyShieldBash : ISpecialAction
{
    public override string Name => "Holy Shield Bash";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25550u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject sourceA = Svc.Objects.FirstOrDefault((IGameObject obj) => obj.BaseId == 12632);
        IGameObject sourceB = Svc.Objects.FirstOrDefault((IGameObject obj) => obj.BaseId == 12601);
        if (sourceA == null || sourceB == null)
        {
            return;
        }
        Data.TetherPlayer.Clear();
        foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => (int)o.ObjectKind == 1))
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 4f,
                radiusZ = 10f,
                drawOnObject = true,
                endToTarget = true,
                target = item,
                delayDrawTime = 2000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 25297u }
                },
                TetherCheck = new TetherCheck
                {
                    CheckType = 1,
                    TetherID = new HashSet<int> { 84 }
                }
            };
            DrawManager.Draw(element, sourceA);
            DrawManager.Draw(element, sourceB);
        }
    }
}
