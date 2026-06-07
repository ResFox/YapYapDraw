using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M4S;

public class Soulshock : ISpecialAction
{
    public override string Name => "Ion Transfer (knockback)";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 20033u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        SimpleElement.ShowText("Go to boss front — prep knockback", (TextGimmickHintStyle)0);
    }
}
