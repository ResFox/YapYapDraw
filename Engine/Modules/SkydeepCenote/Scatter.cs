using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.SkydeepCenote;

public class Scatter : ISpecialAction
{
    public override string Name => "Scatter";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36736u, 36737u };

    public override void OnActionCast(ActorCastInfo info)
    {
        int offset = ((info.ActionId == 36736) ? 8 : (-8));
        foreach (IGameObject actor in Svc.Objects.Where(o =>
        {
            if (o.BaseId == 16852)
            {
                ICharacter character = (ICharacter)((o is ICharacter) ? o : null);
                if (character != null)
                {
                    return character.IsCharacterVisible();
                }
            }
            return false;
        }))
        {
            SimpleElement.Circle(new Vector3(actor.Position.X + (float)offset, actor.Position.Y, actor.Position.Z), 6f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 36738u }
            });
        }
    }
}
