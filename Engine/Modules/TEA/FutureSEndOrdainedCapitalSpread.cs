using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.Memory;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TEA;

public class FutureSEndOrdainedCapitalSpread : ISpecialAction
{
    public override string Name => "Future's End α + Ordained Capital (spread)";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18596u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        IEnumerable<IGameObject> source = Svc.Objects.Where((IGameObject o) => o.BaseId == 11350);
        IGameObject cloestfade = source.MinBy((IGameObject o) => (o.Position - info.Source.Position).LengthSquared());
        IGameObject target = Data.TetherPlayer.FirstOrDefault((TetherInfo tether) => cloestfade.GameObjectId == tether.To).From.GameObject();
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 30f,
            radiusZ = 30f,
            drawOnObject = true,
            delayDrawTime = 6000f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 18528u }
            }
        }, target);
    }
}
