using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.DancingMad.P3;

public class Implosion : ISpecialAction
{
    public override string Name => "Implosion";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47869u, 47870u, 47871u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId != 47869 && info.ActionId != 47870)
            return;

        DrawElement element = new DrawElement
        {
            drawAvfx = "gl_fan090_1bf",
            Position = info.Pos,
            drawOnObject = false,
            radiusX = 40f,
            radiusZ = 40f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 47871u },
                TargetHitCount = 4
            }
        };

        if (info.ActionId == 47869)
        {
            element.refRotation = info.Facing;
            aoes.Add(DrawManager.Draw(element));
            element.refRotation = info.Facing + 180.Degrees();
            aoes.Add(DrawManager.Draw(element));
            element.refRotation = info.Facing + 90.Degrees();
            aoes.Add(DrawManager.Draw(element));
            element.refRotation = info.Facing - 90.Degrees();
            aoes.Add(DrawManager.Draw(element));
        }
        else
        {
            element.refRotation = info.Facing + 90.Degrees();
            aoes.Add(DrawManager.Draw(element));
            element.refRotation = info.Facing - 90.Degrees();
            aoes.Add(DrawManager.Draw(element));
            element.refRotation = info.Facing;
            aoes.Add(DrawManager.Draw(element));
            element.refRotation = info.Facing + 180.Degrees();
            aoes.Add(DrawManager.Draw(element));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 47871 && aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
