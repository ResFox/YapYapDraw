using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P1;

public class Overdrive : ISpecialAction
{
    private ulong _sourceId;

    public override string Name => "Overdrive";

    public override HashSet<uint> ActionID => new HashSet<uint> { 50722u };

    public override void Update()
    {
        if (aoes.Count == 0 || _sourceId == 0)
            return;

        IGameObject? tank = _sourceId.GameObject()?.TargetObject;
        if (tank == null)
            return;

        aoes[0].Enable = true;
        aoes[0].Owner = tank;
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        IGameObject? boss = info.Source ?? Svc.Objects.FirstOrDefault(o => o.BaseId == 19504u);
        if (boss == null)
            return;

        IGameObject? tank = boss.TargetObject ?? info.Target;
        if (tank == null)
            return;

        _sourceId = boss.GameObjectId;
        aoes.Add(DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 5f,
            radiusZ = 5f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 49739u },
                TargetHitCount = 3
            }
        }, tank));
    }

    public override void Reset()
    {
        _sourceId = 0;
        base.Reset();
    }
}
