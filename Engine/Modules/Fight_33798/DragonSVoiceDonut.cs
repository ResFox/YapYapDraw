using System.Collections.Generic;
using Dalamud.Game;
using YapYapDraw.Engine.Interop;
using Lumina.Excel.Sheets;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_33798;

public class DragonSVoiceDonut : ISpecialAction
{
    public override string Name => "Dragon's Voice (donut)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43844u, 43845u, 43846u, 43847u };

    public override void OnActionCast(ActorCastInfo info)
    {
        float duration;
        float delay;
        (duration, delay) = info.ActionId switch
        {
            43844 => (7500, 0), 
            43845 => (2000, 7500), 
            43846 => (2000, 9500), 
            43847 => (2000, 11500), 
            _ => default((int, int)), 
        };
        if (duration != 0f || delay != 0f)
        {
            Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
            DrawElement drawElement = new DrawElement();
            Omen omen = ((Action)(row)).Omen.Value;
            drawElement.drawAvfx = (((Omen)(omen)).Path).ToString();
            drawElement.Position = info.Pos;
            drawElement.drawOnObject = false;
            drawElement.radiusX = (int)((Action)(row)).EffectRange;
            drawElement.radiusZ = (int)((Action)(row)).EffectRange;
            drawElement.delayDrawTime = delay;
            drawElement.destroyTime = duration;
            DrawManager.Draw(drawElement);
        }
    }
}
