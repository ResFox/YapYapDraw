using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DSR;

public class HeavenlyHeel : ISpecialAction
{
    public override string Name => "Heavenly Heel";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25543u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject? source = info.SourceId.GameObject();
        IGameObject target = info.TargetId.GameObject();
        if (source != null && target != null)
        {
            DrawManager.Draw(new DrawElement
            {
                drawType = ElementType.LockOn,
                drawAvfx = "tank_lockon01i",
                drawOnObject = true
            }, target);
        }
    }
}
