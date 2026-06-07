using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class SwordQuiver : ISpecialAction
{
    public override string Name => "Sword Quiver";

    public override uint Phase => 2u;

    public override uint WeatherID => 78u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31540u, 31541u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan090_1bf",
                radiusX = 100f,
                radiusZ = 100f,
                drawOnObject = true,
                target = allPlayer,
                delayDrawTime = (info.CastTime - 1.5f) * 1000f,
                TetherCheck = new TetherCheck
                {
                    CheckType = 1,
                    TetherID = new HashSet<int> { 84 }
                },
                hitCounter = new HitCounter
                {
                    ActionID = ActionID
                }
            }, info.SourceId.GameObject());
        }
    }
}
