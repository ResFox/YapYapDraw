using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M7S;

public class SporeCloud : ISpecialAction
{
    public override string Name => "Spore Cloud";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42379u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawElement element = new DrawElement
        {
            Enable = false,
            Position = info.Pos,
            drawOnObject = false,
            radiusX = 8f,
            radiusZ = 8f,
            destroyTime = 5000f
        };
        aoes.Add(DrawManager.Draw(element));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
