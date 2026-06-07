using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using Lumina.Excel.Sheets;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_43571;

public class RunicCannon : ISpecialAction
{
    public override string Name => "Runic Cannon";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 34959u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        IGameObject target = Svc.Objects.SearchById((ulong)info.TargetId);
        if (source != null && target != null && source.IsHostile())
        {
            Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
            int delay = info.DisplayDelay / 10;
            DrawManager.Draw(new DrawElement
            {
                drawType = ElementType.Omen,
                radiusX = (int)((Action)(row)).EffectRange,
                radiusZ = (int)((Action)(row)).EffectRange,
                target = ((target != source) ? target : null),
                refRotation = info.Facing,
                fixRotation = true,
                drawOnObject = true,
                delayDrawTime = ((delay > 0) ? (delay * 500) : 0),
                destroyTime = (long)(((double)info.CastTime + 0.3 - (double)(delay / 2)) * 1000.0),
                drawAvfx = "customFan",
                refRadian = 4.712f
            }, target, source);
        }
    }
}
