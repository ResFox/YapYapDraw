using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TOP.P5;

public class NearWorld : ISpecialAction
{
    public override string Name => "Near World";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3442)
        {
            IGameObject val = info.TargetID.GameObject();
            if (val != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    radiusX = 8f,
                    radiusZ = 8f,
                    drawOnObject = true,
                    delayDrawTime = (int)(info.Time - 6f) * 1000,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 31625u },
                        HitTarget = val
                    }
                }, val);
            }
        }
    }
}
