using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class SinboundMeltdownHourglassLaser : ISpecialAction
{
    private enum RotationType
    {
        Clockwise,
        Counterclockwise
    }

    private readonly Dictionary<IGameObject, RotationType> records = new Dictionary<IGameObject, RotationType>();

    public override string Name => "Sinbound Meltdown (hourglass laser)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40235u };

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2970)
        {
            switch (info.Stack)
            {
            case 269u:
                records.Add(info.TargetID.GameObject(), RotationType.Counterclockwise);
                break;
            case 348u:
                records.Add(info.TargetID.GameObject(), RotationType.Clockwise);
                break;
            }
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        IGameObject hourglass = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17832 && o.Position.AlmostEqual(info.Source.Position, 1f));
        if (hourglass != null && records.TryGetValue(hourglass, out var rotationType))
        {
            for (int i = 1; i <= 9; i++)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general02xf",
                    radiusX = 2.5f,
                    radiusZ = 50f,
                    drawOnObject = true,
                    refRotation = info.Source.Rotation.Radians() + i * ((rotationType == RotationType.Clockwise) ? (-15.Degrees()) : 15.Degrees()),
                    fixRotation = true,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 40292u },
                        TargetHitCount = i,
                        HitTarget = info.Source
                    }
                }, info.Source);
            }
        }
    }

    public override void Reset()
    {
        records.Clear();
        base.Reset();
    }
}
