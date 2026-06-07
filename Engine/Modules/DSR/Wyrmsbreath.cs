using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DSR;

public class Wyrmsbreath : ISpecialAction
{
    private static bool holyGlow = false;

    private static bool darkGlow = false;

    private static IGameObject?[] Dragons = (IGameObject?[])new IGameObject[2];

    private static bool DrawTether = true;

    public override string Name => "Wyrmsbreath";

    public override uint Phase => 6u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 27954u, 27955u, 27956u, 27957u };

    public override void OnActionCast(ActorCastInfo info)
    {
        base.NumCasts++;
        switch (info.ActionId)
        {
        case 27954:
            darkGlow = false;
            Dragons[0] = info.SourceId.GameObject();
            break;
        case 27955:
            darkGlow = true;
            Dragons[0] = info.SourceId.GameObject();
            break;
        case 27956:
            holyGlow = false;
            Dragons[1] = info.SourceId.GameObject();
            break;
        case 27957:
            holyGlow = true;
            Dragons[1] = info.SourceId.GameObject();
            break;
        }
        if (base.NumCasts % 2 != 0 || base.NumCasts == 0)
        {
            return;
        }
        if (holyGlow && darkGlow)
        {
            IGameObject dragon = Dragons[0];
            if (dragon != null)
            {
                IGameObject targetObject = dragon.TargetObject;
                if (targetObject != null)
                {
                    DrawManager.Draw(new DrawElement
                    {
                        drawType = ElementType.LockOn,
                        drawAvfx = "m0618trg_a0k1",
                        drawOnObject = true
                    }, targetObject);
                }
            }
        }
        else if (holyGlow)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan030_1bf",
                radiusX = 50f,
                radiusZ = 50f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 27964u }
                }
            }, Dragons[1]);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 15f,
                radiusZ = 15f,
                drawOnObject = true,
                alwaysDrawOnCurrentTarget = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 27965u }
                }
            }, Dragons[0]);
        }
        else
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan030_1bf",
                radiusX = 50f,
                radiusZ = 50f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 27963u }
                }
            }, Dragons[0]);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 15f,
                radiusZ = 15f,
                drawOnObject = true,
                alwaysDrawOnCurrentTarget = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 27965u }
                }
            }, Dragons[1]);
        }
    }

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        bool isTether = Id - 194 <= 2;
        if (isTether && actorId == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId && DrawTether)
        {
            DrawTether = false;
            SimpleElement.FanToTarget(targetId, actorId, 100f, 10, Follow: true, default, 3000f, 27958u);
        }
    }

    public override void Reset()
    {
        Dragons = (IGameObject?[])new IGameObject[2];
        DrawTether = true;
        base.Reset();
    }
}
