using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.SpheneDarkEx;
public class GreatCross : ISpecialAction
{
    public override string Name => "Great Cross";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public unsafe override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        IGameObject source = actorId.GameObject();
        IGameObject target = targetId.GameObject();
        uint baseId = source.BaseId;
        Angle angle = baseId switch
        {
            18761u => 41f.Degrees(), 
            18708u => -153f.Degrees(), 
            _ => default, 
        };
        if (angle != default)
        {
            WPos wPos = new WPos(100f, 100f);
            SimpleElement.Rectangle(wPos.ToVec3(), 50f, 2f, 50f, Angle.FromDirection(wPos - new WPos(source.Position)) + angle, (baseId == 18761) ? 7600 : 5600);
            var address = (Character*)source.Address;
            address->GameObject.RenderFlags = (VisibilityFlags)((int)address->GameObject.RenderFlags | 2 | 0x800);
            var address2 = (Character*)target.Address;
            address2->GameObject.RenderFlags = (VisibilityFlags)((int)address2->GameObject.RenderFlags | 2 | 0x800);
        }
    }
}
