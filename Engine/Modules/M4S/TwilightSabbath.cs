using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4S;

public class TwilightSabbath : ISpecialAction
{
    public override string Name => "Twilight Sabbath";

    public override uint Phase => 6u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (source.BaseId == 17323)
        {
            var (angle, delay, half) = id switch
            {
                4566u => (-90.Degrees(), 0, true), 
                4567u => (-90.Degrees(), 8300, false), 
                4568u => (90.Degrees(), 0, true), 
                4569u => (90.Degrees(), 8300, false), 
                _ => default((Angle, int, bool)), 
            };
            if (angle != default)
            {
                SimpleElement.Fan(source, 60f, 180, source.Rotation.Radians() + angle, 3000f, delay, new HitCounter
                {
                    ActionID = new HashSet<uint> { 38441u, 38442u },
                    TargetHitCount = (half ? 2 : 4)
                });
            }
        }
    }
}
