using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.FRU;

public class ShiningArmor : ISpecialAction
{
    public override string Name => "Shining Armor (look away)";

    public override uint Phase => 2u;

    public override uint WeatherID => 35u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40209u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        base.NumCasts++;
        if (base.NumCasts == 1)
        {
            IGameObject castObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17823);
            DrawManager.Draw(new DrawElement
            {
                drawType = ElementType.Channeling,
                drawAvfx = "chn_chainlightning_3t1",
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40185u }
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer, castObject);
            DrawManager.Draw(new DrawElement
            {
                drawType = ElementType.Channeling,
                drawAvfx = "chn_miruna1v",
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40185u }
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer, castObject);
        }
    }
}
