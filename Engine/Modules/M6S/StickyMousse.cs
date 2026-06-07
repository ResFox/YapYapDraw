using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M6S;

public class StickyMousse : ISpecialAction
{
    public override string Name => "Sticky Mousse";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42645u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject member in PlayerHelper.Healer.Union(PlayerHelper.DPS))
        {
            SimpleElement.Circle(member, 4f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 42646u }
            });
        }
    }

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 4453)
        {
            IGameObject target = info.TargetID.GameObject();
            SimpleLockon.ShareLockon(target);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bpxf",
                radiusX = 4f,
                radiusZ = 4f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 42647u }
                }
            }, target);
        }
    }
}
