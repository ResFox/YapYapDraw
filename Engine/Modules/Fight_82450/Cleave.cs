using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_82450;

public class Cleave : ISpecialAction
{
    public override string Name => "Cleave";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36011u, 36012u, 36013u, 35977u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        ushort actionId = info.ActionId;
        if ((uint)(actionId - 36011) <= 2u)
        {
            base.NumCasts++;
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "gl_fan180_1bf",
                radiusX = 20f,
                radiusZ = 20f,
                drawOnObject = true,
                refRotation = info.Facing,
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 35977u, 35978u, 35979u, 35980u, 35981u, 35982u }
                }
            };
            if (base.NumCasts == 1)
            {
                DrawManager.Draw(drawElement, info.SourceId.GameObject());
                return;
            }
            DrawQueue.Enqueue((new HashSet<uint> { 35977u, 35978u, 35979u, 35980u, 35981u, 35982u }, new(IGameObject, DrawElement[])[1] { (info.SourceId.GameObject(), new DrawElement[1] { drawElement }) }));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 35977)
        {
            base.NumCasts = 0;
        }
    }
}
