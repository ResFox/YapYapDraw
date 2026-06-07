using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M1S;

public class Mouser : ISpecialAction
{
    public override string Name => "Mouser";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37955u, 39276u, 38054u };

    public override void OnActionCast(ActorCastInfo info)
    {
        ushort actionId = info.ActionId;
        if (actionId == 37955 || actionId == 39276)
        {
            base.NumCasts++;
            IGameObject source = info.SourceId.GameObject();
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "customRect2",
                Position = source.Position,
                drawOnObject = false,
                radiusX = 5f,
                radiusZ = 5f,
                fixRotation = true,
                refColor = GroundOmen.enemyColor,
                refTargetColor = GroundOmen.enemyColor,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38054u },
                    TargetHitCount = base.NumCasts
                }
            };
            if (info.ActionId == 39276)
            {
                drawElement.refColor = GroundOmen.Red;
                drawElement.refTargetColor = GroundOmen.Red;
            }
            DrawManager.Draw(drawElement, source);
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 38054)
        {
            base.NumCasts = 0;
        }
    }
}
