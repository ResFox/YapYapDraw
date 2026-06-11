using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DancingMad.P5;

public class Meteor : ISpecialAction
{
    private ulong _sourceId;

    public override string Name => "Meteor";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47952u, 47954u };

    public override void Update()
    {
        if (aoes.Count != 2)
            return;

        IGameObject[] order = PlayerHelper.RaidByEnmity(_sourceId).Take(2).ToArray();
        for (int i = 0; i < 2; i++)
        {
            IGameObject? target = i < order.Length ? order[i] : null;
            aoes[i].Enable = target != null;
            aoes[i].Owner = target;
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId != 47952)
            return;

        _sourceId = info.SourceId;
        DrawElement element = new DrawElement
        {
            drawAvfx = "general_1bzt",
            radiusX = 5f,
            radiusZ = 5f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 47954u }
            }
        };
        aoes.Add(DrawManager.Draw(element, Svc.Objects.LocalPlayer));
        aoes.Add(DrawManager.Draw(element, Svc.Objects.LocalPlayer));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 47954)
            aoes.Clear();
    }
}
