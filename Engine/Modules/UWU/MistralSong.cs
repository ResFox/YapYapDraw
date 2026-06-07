using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.UWU;

public class MistralSong : ISpecialAction
{
    private static List<IGameObject> Adds = new List<IGameObject>();

    public override string Name => "Mistral Song";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject source, uint icon, ulong TargetID)
    {
        if (Svc.Objects.Where(o =>
        {
            if (o.BaseId == 8723)
            {
                ICharacter character = (ICharacter)((o is ICharacter) ? o : null);
                if (character != null)
                {
                    return character.IsCharacterVisible();
                }
            }
            return false;
        }).Count() == 2 && Adds.Count == 0)
        {
            Adds = Svc.Objects.Where((IGameObject o) => o.BaseId == 8723).ToList();
        }
        if (icon != 16)
        {
            return;
        }
        DrawElement element = new DrawElement
        {
            drawAvfx = "general02pxf",
            radiusX = 5f,
            radiusZ = 40f,
            drawOnObject = true,
            target = source,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 11074u, 11083u }
            }
        };
        if (Adds.Count == 0)
        {
            DrawManager.Draw(element, Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 8722));
        }
        else
        {
            DrawManager.Draw(element, Adds[0]);
            Adds.RemoveAt(0);
        }
    }

    public override void Reset()
    {
        Adds.Clear();
        base.Reset();
    }
}
