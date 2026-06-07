using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.UWU;

public class Incinerate : ISpecialAction
{
    public override string Name => "Incinerate (cone)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnChatMessage(uint chatType, string content)
    {
        IGameObject target = Svc.Objects.Where((IGameObject o) => o.BaseId == 8730 && o.IsTargetable).FirstOrDefault();
        if (chatType == 68)
        {
            InstanceContentTextData row = Svc.Data.GetExcelSheet<InstanceContentTextData>((ClientLanguage?)null, (string)null).GetRow(2600u);
            ReadOnlySeString text = ((InstanceContentTextData)(row)).Text;
            if (content == ((ReadOnlySeString)(text)).ExtractText())
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "gl_fan120_1bf",
                    radiusX = 15f,
                    radiusZ = 15f,
                    drawOnObject = true,
                    alwaysFaceCurrentTarget = true,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 11094u },
                        TargetHitCount = 3
                    }
                }, target);
            }
        }
    }
}
