using System.Collections.Generic;
using Dalamud.Game;
using YapYapDraw.Engine.Interop;
using Lumina.Excel.Sheets;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.CE107;

public class HoppingMad : ISpecialAction
{
    public override string Name => "Hopping Mad";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37323u, 30872u, 30873u, 37041u };

    public override void OnActionCast(ActorCastInfo info)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        byte effectRange = ((Action)(row)).EffectRange;
        SimpleElement.Donut(info.SourceId.GameObject(), (int)effectRange, 60f, 3500f, info.CastTime * 1000f - 2000f);
    }
}
