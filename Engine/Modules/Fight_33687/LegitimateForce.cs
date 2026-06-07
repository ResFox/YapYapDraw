using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.Fight_33687;

public class LegitimateForce : ISpecialAction
{
    public override string Name => "Legitimate Force";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36638u, 36639u, 36640u, 36641u, 36642u, 36643u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 36638:
            AddAOEs(info.SourceId.GameObject(), -90.Degrees(), -90.Degrees());
            break;
        case 36639:
            AddAOEs(info.SourceId.GameObject(), -90.Degrees(), 90.Degrees());
            break;
        case 36640:
            AddAOEs(info.SourceId.GameObject(), 90.Degrees(), 90.Degrees());
            break;
        case 36641:
            AddAOEs(info.SourceId.GameObject(), 90.Degrees(), -90.Degrees());
            break;
        }
        void AddAOEs(IGameObject source, Angle first, Angle second)
        {
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "general02wf",
                radiusX = 40f,
                radiusZ = 20f,
                drawOnObject = true,
                refRotation = info.Facing,
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = ActionID,
                    TargetHitCount = 2
                }
            };
            drawElement.refRotation = info.Facing + first;
            aoes.Add(DrawManager.Draw(drawElement, source));
            drawElement.refRotation = info.Facing + second;
            aoes.Add(DrawManager.Draw(drawElement, source));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
