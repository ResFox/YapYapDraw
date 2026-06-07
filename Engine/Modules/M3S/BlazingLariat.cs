using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M3S;

public class BlazingLariat : ISpecialAction
{
    public override string Name => "Blazing Lariat (stack / spread cone)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37848u, 37849u, 37850u, 37851u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 37848:
        case 37849:
            PlayerHelper.AllPlayers.ForEach(player =>
            {
                SimpleElement.FanToTarget(info.SourceId, player.EntityId, 40f, 45, Follow: true, default, 3000f, 37852u);
            });
            break;
        case 37850:
        case 37851:
        {
            List<IGameObject> tank = PlayerHelper.Tank;
            List<IGameObject> healer = PlayerHelper.Healer;
            int offset = 0;
            IGameObject[] combined = (IGameObject[])new IGameObject[tank.Count + healer.Count];
            Span<IGameObject> tankSpan = CollectionsMarshal.AsSpan(tank);
            tankSpan.CopyTo(new Span<IGameObject>(combined).Slice(offset, tankSpan.Length));
            offset += tankSpan.Length;
            Span<IGameObject> healerSpan = CollectionsMarshal.AsSpan(healer);
            healerSpan.CopyTo(new Span<IGameObject>(combined).Slice(offset, healerSpan.Length));
            offset += healerSpan.Length;
            combined = combined;
            foreach (IGameObject target in combined)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "gl_fan020_0pt",
                    radiusX = 40f,
                    radiusZ = 40f,
                    drawOnObject = true,
                    target = target,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 37853u }
                    }
                }, info.SourceId.GameObject());
            }
            break;
        }
        }
    }
}
