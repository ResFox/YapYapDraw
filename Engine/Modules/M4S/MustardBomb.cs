using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M4S;

public class MustardBomb : ISpecialAction
{
    public override string Name => "Mustard Bomb (tether spread)";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38430u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject member in PlayerHelper.DPS.Union(PlayerHelper.Healer))
        {
            SimpleLockon.TarLockOn6m5s(member, (info.CastTime - 5f) * 1000f);
        }
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 6f,
            radiusZ = 6f,
            drawOnObject = true,
            TetherCheck = new TetherCheck
            {
                CheckType = 0,
                TetherID = new HashSet<int> { 283 }
            },
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 38432u },
                TargetHitCount = 2
            }
        }, PlayerHelper.AllPlayers);
    }
}
