using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP.P5;

public class P3InOutHotWingsCross : ISpecialAction
{
    public override string Name => "P3 In/Out + Hot Wings Cross";

    public override uint Phase => 5u;

    public override uint WeatherID => 174u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 32789u, 32374u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 32789)
        {
            base.NumCasts = 0;
            base.CanDraw = true;
        }
        if (info.ActionId == 32374)
        {
            base.CanDraw = false;
        }
    }

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (id != 7747 || !base.CanDraw || base.NumCasts == 5)
        {
            return;
        }
        base.NumCasts++;
        switch (source.BaseId)
        {
        case 15721u:
        {
            ICharacter character = (ICharacter)((source is ICharacter) ? source : null);
            if (character != null && character.GetTransformationID() == 4)
            {
                DrawElement donut = new DrawElement
                {
                    drawAvfx = "customDonut",
                    refRadian = 0.25f,
                    radiusX = 40f,
                    radiusZ = 40f,
                    drawOnObject = true,
                    refColor = GroundOmen.enemyColor,
                    refTargetColor = GroundOmen.enemyColor,
                    destroyTime = 13100f
                };
                if (base.NumCasts > 3)
                {
                    donut.delayDrawTime = 9100f;
                    donut.destroyTime = 4100f;
                }
                DrawManager.Draw(donut, source);
            }
            else
            {
                DrawElement circle = new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    radiusX = 10f,
                    radiusZ = 10f,
                    drawOnObject = true,
                    destroyTime = 13100f
                };
                if (base.NumCasts > 3)
                {
                    circle.delayDrawTime = 9100f;
                    circle.destroyTime = 4100f;
                }
                DrawManager.Draw(circle, source);
            }
            break;
        }
        case 15722u:
        {
            ICharacter character = (ICharacter)((source is ICharacter) ? source : null);
            if (character != null && character.GetTransformationID() == 4)
            {
                DrawElement wingLeft = new DrawElement
                {
                    drawAvfx = "general_x02f",
                    radiusX = 18f,
                    radiusZ = 80f,
                    refOffsetX = 22f,
                    drawOnObject = true,
                    destroyTime = 13100f
                };
                DrawElement wingRight = new DrawElement
                {
                    drawAvfx = "general_x02f",
                    radiusX = 18f,
                    radiusZ = 80f,
                    refOffsetX = -22f,
                    drawOnObject = true,
                    destroyTime = 13100f
                };
                if (base.NumCasts > 3)
                {
                    wingLeft.delayDrawTime = 9100f;
                    wingLeft.destroyTime = 4100f;
                    wingRight.delayDrawTime = 9100f;
                    wingRight.destroyTime = 4100f;
                }
                DrawManager.Draw(wingLeft, source);
                DrawManager.Draw(wingRight, source);
            }
            else
            {
                DrawElement crossA = new DrawElement
                {
                    drawAvfx = "general_x02f",
                    radiusX = 5f,
                    radiusZ = 100f,
                    drawOnObject = true,
                    destroyTime = 13100f
                };
                DrawElement crossB = new DrawElement
                {
                    drawAvfx = "general_x02f",
                    radiusX = 5f,
                    radiusZ = 100f,
                    drawOnObject = true,
                    refRotation = 90.Degrees(),
                    destroyTime = 13100f
                };
                if (base.NumCasts > 3)
                {
                    crossA.delayDrawTime = 9100f;
                    crossA.destroyTime = 4100f;
                    crossB.delayDrawTime = 9100f;
                    crossB.destroyTime = 4100f;
                }
                DrawManager.Draw(crossA, source);
                DrawManager.Draw(crossB, source);
            }
            break;
        }
        }
    }
}
