using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.P12S.P12S;

public class UmbralImpact : ISpecialAction
{
    public override string Name => "Umbral Impact Sigil (buff)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3581)
        {
            IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 16171);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "customRect",
                radiusX = 3f,
                radiusZ = 100f,
                drawOnObject = true,
                target = info.TargetID.GameObject(),
                refColor = Vector4.One,
                refTargetColor = Vector4.One,
                delayDrawTime = (info.Time - 5f) * 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 33544u }
                }
            }, target);
        }
    }
}
