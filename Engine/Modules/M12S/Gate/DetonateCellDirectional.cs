using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M12S.Gate;

public class DetonateCellDirectional : ISpecialAction
{
    public override string Name => "Detonate Cell: Directional";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3558)
        {
            new DrawElement
            {
                drawAvfx = "m695_est_north_exr_t0p",
                drawOnObject = true
            };
            switch (info.Stack)
            {
            case 1036u:
            {
                IGameObject? target = info.TargetID.GameObject();
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 46255u }
                };
                SimpleElement.Fan(target, 60f, 45, default, 3000f, 11000f, hitCounter, fixRotation: false);
                break;
            }
            case 1037u:
                SimpleElement.Fan(info.TargetID.GameObject(), 60f, 45, -90.Degrees(), 3000f, 11000f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 46255u }
                }, fixRotation: false);
                break;
            case 1038u:
                SimpleElement.Fan(info.TargetID.GameObject(), 60f, 45, -180.Degrees(), 3000f, 11000f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 46255u }
                }, fixRotation: false);
                break;
            case 1039u:
                SimpleElement.Fan(info.TargetID.GameObject(), 60f, 45, -270.Degrees(), 3000f, 11000f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 46255u }
                }, fixRotation: false);
                break;
            }
        }
    }
}
