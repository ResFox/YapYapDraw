using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class GroundAoESpread : ISpecialAction
{
    public override string Name => "Ground AoESpread";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3425)
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 6f,
                radiusZ = 6f,
                drawOnObject = true,
                delayDrawTime = (info.Time - 5f) * 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31571u }
                }
            };
            if (info.TargetID != ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
            {
                DrawManager.Draw(element, info.TargetID.GameObject());
            }
        }
    }
}
