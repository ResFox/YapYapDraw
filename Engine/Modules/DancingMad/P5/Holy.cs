using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P5;

public class Holy : ISpecialAction
{
    private ulong _sourceId;

    public override string Name => "Holy";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47952u, 47956u };

    public override void Update()
    {
        if (aoes.Count != 6)
            return;

        IGameObject[] order = PlayerHelper.RaidByEnmity(_sourceId).Skip(2).ToArray();
        for (int i = 0; i < 6; i++)
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
        for (int i = 0; i < 6; i++)
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 5f,
                radiusZ = 5f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47956u }
                }
            };
            aoes.Add(DrawManager.Draw(element, Svc.Objects.LocalPlayer));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 47956)
            aoes.Clear();

        if (info.ActionId != 47952)
            return;

        foreach (IGameObject player in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 5f,
                radiusZ = 5f,
                distanceCheck = new DistanceCheck
                {
                    CheckType = 2,
                    CheckObject = _sourceId.GameObject()
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47955u }
                }
            }, player);
        }
    }
}
