using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4S;

public class FlameSlashLine : ISpecialAction
{
    public override string Name => "Flame Slash (line)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38789u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 2.5f,
                radiusZ = 60f,
                drawOnObject = true,
                target = allPlayer,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38790u },
                    TargetHitCount = 13
                }
            }, info.SourceId.GameObject());
        }
    }
}
