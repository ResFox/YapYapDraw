using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P1;

public class FerociousLaceration : ISpecialAction
{
    private ulong _sourceId;
    private IGameObject? _target;
    private bool _secondTB;

    public override string Name => "Ferocious Laceration";

    public override HashSet<uint> ActionID => new HashSet<uint> { 50179u, 50401u };

    public override void Update()
    {
        if (aoes.Count == 0)
            return;

        aoes[0].Target = _secondTB
            ? PlayerHelper.RaidByEnmity(_sourceId).Skip(1).FirstOrDefault()
            : _target;
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        _sourceId = info.SourceId;
        _target = info.TargetId.GameObject();
        DrawElement element = new DrawElement
        {
            drawAvfx = "gl_fan120_1bf",
            radiusX = 100f,
            radiusZ = 100f,
            refColor = GroundOmen.Red,
            refTargetColor = GroundOmen.Red,
            target = info.TargetId.GameObject(),
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 50179u, 50401u },
                TargetHitCount = 2
            }
        };
        aoes.Add(DrawManager.Draw(element, info.SourceId.GameObject()));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 50179)
            _secondTB = true;
        if (info.ActionId == 50401)
        {
            _sourceId = 0;
            _secondTB = false;
        }
    }

    public override void Reset()
    {
        _sourceId = 0;
        _secondTB = false;
        base.Reset();
    }
}
