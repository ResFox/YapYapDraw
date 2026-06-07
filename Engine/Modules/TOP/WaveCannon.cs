using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class WaveCannon : ISpecialAction
{
    public override string Name => "Wave Cannon";

    public override uint Phase => 6u;

    public override uint WeatherID => 175u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31657u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => (int)o.ObjectKind == 1 && o.GameObjectId != ((IGameObject)Player.Object).GameObjectId))
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 4f,
                radiusZ = 100f,
                drawOnObject = true,
                target = item,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31659u },
                    TargetHitCount = 8
                }
            }, info.SourceId.GameObject());
        }
    }
}
