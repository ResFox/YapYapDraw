using System.Collections.Generic;
using Dalamud.Game;
using YapYapDraw.Engine.Interop;
using Lumina.Excel.Sheets;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.LockWyvernEx;
public class DragonSVoiceDonut : ISpecialAction
{
    public override string Name => "Dragon's Voice (donut)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43911u, 43912u, 43913u, 43914u };

    public override void OnActionCast(ActorCastInfo info)
    {
        float destroyMs;
        float delayMs;
        (destroyMs, delayMs) = info.ActionId switch
        {
            43911 => (7500, 0), 
            43912 => (2000, 7500), 
            43913 => (2000, 9500), 
            43914 => (2000, 11500), 
            _ => default((int, int)), 
        };
        if (destroyMs != 0f || delayMs != 0f)
        {
            Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
            DrawElement drawElement = new DrawElement();
            Omen value = ((Action)(row)).Omen.Value;
            drawElement.drawAvfx = (((Omen)(value)).Path).ToString();
            drawElement.Position = info.Pos;
            drawElement.drawOnObject = false;
            drawElement.radiusX = (int)((Action)(row)).EffectRange;
            drawElement.radiusZ = (int)((Action)(row)).EffectRange;
            drawElement.delayDrawTime = delayMs;
            drawElement.destroyTime = destroyMs;
            DrawManager.Draw(drawElement);
        }
    }
}
