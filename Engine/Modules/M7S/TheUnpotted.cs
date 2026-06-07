using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M7S;

public class TheUnpotted : ISpecialAction
{
    public override string Name => "The Unpotted";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42362u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan030_1bf",
                radiusX = 60f,
                radiusZ = 60f,
                drawOnObject = true,
                target = allPlayer,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 42363u }
                }
            }, info.SourceId.GameObject());
        }
    }
}
