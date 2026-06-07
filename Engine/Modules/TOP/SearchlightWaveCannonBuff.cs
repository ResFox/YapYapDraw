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

public class SearchlightWaveCannonBuff : ISpecialAction
{
    public override string Name => "Searchlight Wave Cannon (buff)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        uint statusID = info.StatusID;
        bool isSearchlight = statusID - 3452 <= 1;
        if (isSearchlight && info.TargetID == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02pxf",
                radiusX = 50f,
                radiusZ = 50f,
                drawOnObject = true,
                refRotation = ((info.StatusID == 3452) ? (-90.Degrees()) : 90.Degrees()),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31597u }
                }
            }, info.TargetID.GameObject());
        }
    }
}
