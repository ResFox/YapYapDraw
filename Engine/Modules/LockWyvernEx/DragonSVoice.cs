using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.LockWyvernEx;
public class DragonSVoice : ISpecialAction
{
    private Vector3 Origin;

    public override string Name => "Dragon's Voice";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43891u, 45088u, 43892u, 45085u };

    public override void OnActionCast(ActorCastInfo info)
    {
        ushort actionId = info.ActionId;
        if ((uint)(actionId - 43891) <= 1u || actionId == 45085 || actionId == 45088)
        {
            SimpleElement.Rectangle(info);
            Origin = info.Pos;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        if (actionId - 43891 <= 1 || actionId == 45085 || actionId == 45088)
        {
            WDir backOffset = -14f * info.Rotation.ToDirection();
            uint castId = info.ActionId;
            bool useRightOrtho = castId == 43892 || castId == 45088;
            WDir sideOffset = (useRightOrtho ? backOffset.OrthoR() : backOffset.OrthoL());
            SimpleElement.Rectangle(Origin + sideOffset.ToVec3(), 80f, 14f, 0f, info.Rotation, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 43898u, 45093u, 43897u, 45096u }
            });
        }
    }
}
