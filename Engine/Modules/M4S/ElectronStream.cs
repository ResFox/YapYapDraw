using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4S;

public class ElectronStream : ISpecialAction
{
    private const uint Positron = 4000u;

    private const uint Negatron = 4001u;

    private const uint Far = 4002u;

    private const uint Near = 4003u;

    private const uint Spinning = 4004u;

    private const uint RoundHouse = 4005u;

    private const uint Colider = 4006u;

    public override string Name => "Electron Stream";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38360u, 38361u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = ((info.ActionId == 38360) ? "general02xf" : "general02pxf"),
            radiusX = 5f,
            radiusZ = 40f,
            refRotation = info.Facing,
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        }, info.SourceId.GameObject());
        if (((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(4000u))
        {
            SimpleElement.ShowText("Take blue AoE", (TextGimmickHintStyle)0);
        }
        else if (((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(4001u))
        {
            SimpleElement.ShowText("Take yellow AoE", (TextGimmickHintStyle)0);
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId != 38360)
        {
            return;
        }
        new TimeHelper(100L, () =>
        {
            IGameObject cluster = Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 17322);
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "co_trap00h1",
                Position = cluster.Position,
                drawOnObject = false,
                radiusX = 1f,
                radiusY = 5f,
                radiusZ = 1f,
                refOffsetX = 2f,
                refOffsetZ = -2f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38362u, 38363u, 38364u }
                }
            };
            DrawElement drawElement2 = new DrawElement
            {
                drawAvfx = "share_trap01k1",
                Position = cluster.Position,
                drawOnObject = false,
                radiusX = 2f,
                radiusY = 5f,
                radiusZ = 2f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38362u, 38363u, 38364u }
                }
            };
            if (((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(4004u) || ((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(4005u))
            {
                drawElement.refOffsetX = -2.5f;
                drawElement.refOffsetZ = 2.5f;
                DrawManager.Draw(drawElement, cluster);
                drawElement.refOffsetX = -2.5f;
                drawElement.refOffsetZ = -2.5f;
                DrawManager.Draw(drawElement, cluster);
                drawElement.refOffsetX = 2.5f;
                drawElement.refOffsetZ = 2.5f;
                DrawManager.Draw(drawElement, cluster);
                drawElement.refOffsetX = 2.5f;
                drawElement.refOffsetZ = -2.5f;
                DrawManager.Draw(drawElement, cluster);
            }
            else if (((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(4002u) || ((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(4003u) || ((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(4006u))
            {
                drawElement2.refOffsetZ = 5f;
                DrawManager.Draw(drawElement2, cluster);
                drawElement2.refOffsetZ = -5f;
                DrawManager.Draw(drawElement2, cluster);
            }
            List<IBattleChara> players = PlayerHelper.AllPlayers.Cast<IBattleChara>().ToList();
            bool anyNear = players.Any((IBattleChara x) => x.StatusList.Any((IStatus status) => status.StatusId == 4003));
            foreach (IBattleChara carrier in players.Where((IBattleChara x) => x.StatusList.Any(status =>
            {
                uint statusId = status.StatusId;
                return statusId - 4002 <= 1;
            })).ToList())
            {
                foreach (IBattleChara other in players)
                {
                    if (carrier != other)
                    {
                        DrawManager.Draw(new DrawElement
                        {
                            drawAvfx = "customFan",
                            refRadian = 15.Degrees().Rad,
                            drawOnObject = true,
                            target = (IGameObject?)other,
                            radiusX = 50f,
                            radiusZ = 50f,
                            refColor = GroundOmen.friendColor,
                            refTargetColor = GroundOmen.friendColor,
                            distanceCheck = new DistanceCheck
                            {
                                CheckObject = (IGameObject?)carrier,
                                CheckType = ((!anyNear) ? 1 : 0)
                            },
                            hitCounter = new HitCounter
                            {
                                ActionID = new HashSet<uint> { 38362u }
                            }
                        }, (IGameObject?)carrier);
                    }
                }
            }
            foreach (IBattleChara spinner in players.Where((IBattleChara x) => x.StatusList.Any((IStatus status) => status.StatusId == 4004)).ToList())
            {
                SimpleElement.Circle((IGameObject?)spinner, 2f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 38363u }
                });
            }
            foreach (IBattleChara rounder in players.Where((IBattleChara x) => x.StatusList.Any((IStatus status) => status.StatusId == 4005)).ToList())
            {
                SimpleElement.Donut((IGameObject?)rounder, 10f, 25f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 38364u }
                });
            }
        });
    }
}
