using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Zelenia;

public class SpecterOfTheLost : ISpecialAction
{
    public override string Name => "SpecterOfTheLost";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 43167u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan045_1bf",
                radiusX = 48f,
                radiusZ = 48f,
                drawOnObject = true,
                target = allPlayer,
                TetherCheck = new TetherCheck
                {
                    CheckType = 0,
                    TetherID = new HashSet<int>()
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 43168u },
                    TargetHitCount = 2
                }
            }, info.SourceId.GameObject());
        }
    }
}
