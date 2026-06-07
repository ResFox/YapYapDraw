using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_56456;

public class GoodNaturedFriends : ISpecialAction
{
    public override string Name => "Good-natured Friends";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36533u };

    public override uint Phase => 1u;

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            radiusX = 1f,
            radiusZ = 6f,
            drawOnObject = true,
            OnlyVisible = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 36527u }
            }
        }, info.Source);
    }
}
