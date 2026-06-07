using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.UnendingCoil;

public class NaelQuotes : ISpecialAction
{
    private static readonly DrawElement LunarDynamo = new DrawElement
    {
        drawAvfx = "customDonut",
        refRadian = 0.27272728f,
        radiusX = 22f,
        radiusZ = 22f,
        drawOnObject = true,
        refColor = GroundOmen.enemyColor,
        refTargetColor = GroundOmen.enemyColor,
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 9916u }
        }
    };

    private static readonly DrawElement IronChariot = new DrawElement
    {
        drawAvfx = "general_1bxf",
        radiusX = 8.55f,
        radiusZ = 8.55f,
        drawOnObject = true,
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 9915u }
        }
    };

    private static readonly DrawElement ThermionicBeam = new DrawElement
    {
        drawAvfx = "general_1bpxf",
        radiusX = 4f,
        radiusZ = 4f,
        drawOnObject = true,
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 9917u }
        }
    };

    private static readonly DrawElement RavenDive = new DrawElement
    {
        drawAvfx = "general_1bxf",
        radiusX = 4f,
        radiusZ = 4f,
        drawOnObject = true,
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 9918u }
        }
    };

    private static readonly DrawElement MeteorStream = new DrawElement
    {
        drawAvfx = "general_1bxf",
        radiusX = 4f,
        radiusZ = 4f,
        drawOnObject = true,
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 9920u }
        }
    };

    private static readonly DrawElement DalamudDive = new DrawElement
    {
        drawAvfx = "general_1bxf",
        radiusX = 5f,
        radiusZ = 5f,
        drawOnObject = true,
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 9921u }
        }
    };

    public override string Name => "Nael Quotes";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnNpcYell(ulong SourceID, ushort Message)
    {
        IGameObject nael = Svc.Objects.FirstOrDefault((IGameObject g) => g.BaseId == 8161);
        switch (Message)
        {
        case 6492:
            DrawManager.Draw(LunarDynamo, nael);
            DrawQueue.Enqueue((new HashSet<uint> { 9916u }, new(IGameObject, DrawElement[])[1] { (nael, new DrawElement[1] { IronChariot }) }));
            break;
        case 6493:
            DrawManager.Draw(LunarDynamo, nael);
            ActionQueue.Enqueue((new HashSet<uint> { 9916u }, delegate
            {
                SimpleLockon.ShareLockon((IGameObject?)Svc.Objects.LocalPlayer);
            }));
            break;
        case 6494:
            SimpleLockon.ShareLockon((IGameObject?)Svc.Objects.LocalPlayer);
            DrawQueue.Enqueue((new HashSet<uint> { 9917u }, new(IGameObject, DrawElement[])[1] { (nael, new DrawElement[1] { IronChariot }) }));
            break;
        case 6495:
            SimpleLockon.ShareLockon((IGameObject?)Svc.Objects.LocalPlayer);
            DrawQueue.Enqueue((new HashSet<uint> { 9917u }, new(IGameObject, DrawElement[])[1] { (nael, new DrawElement[1] { LunarDynamo }) }));
            break;
        case 6496:
            DrawManager.Draw(RavenDive, PlayerHelper.AllPlayers);
            DrawQueue.Enqueue((new HashSet<uint> { 9918u }, new(IGameObject, DrawElement[])[1] { (nael, Array.Empty<DrawElement>()) }));
            break;
        case 6497:
            DrawManager.Draw(RavenDive, PlayerHelper.AllPlayers);
            DrawQueue.Enqueue((new HashSet<uint> { 9918u }, new(IGameObject, DrawElement[])[1] { (nael, new DrawElement[1] { LunarDynamo }) }));
            break;
        case 6500:
            DrawManager.Draw(MeteorStream, PlayerHelper.AllPlayers);
            DrawQueue.Enqueue((new HashSet<uint> { 9920u }, new(IGameObject, DrawElement[])[1] { (nael.TargetObject, new DrawElement[1] { DalamudDive }) }));
            break;
        case 6501:
            DrawManager.Draw(DalamudDive, nael.TargetObject);
            ActionQueue.Enqueue((new HashSet<uint> { 9921u }, delegate
            {
                SimpleLockon.ShareLockon((IGameObject?)Svc.Objects.LocalPlayer);
            }));
            break;
        case 6502:
            DrawManager.Draw(RavenDive, PlayerHelper.AllPlayers);
            DrawQueue.Enqueue((new HashSet<uint> { 9918u }, new(IGameObject, DrawElement[])[1] { (nael, new DrawElement[1] { LunarDynamo }) }));
            ActionQueue.Enqueue((new HashSet<uint> { 9905u }, delegate
            {
                DrawManager.Draw(MeteorStream, PlayerHelper.AllPlayers);
            }));
            break;
        case 6503:
            DrawManager.Draw(LunarDynamo, nael);
            ActionQueue.Enqueue((new HashSet<uint> { 9916u }, delegate
            {
                DrawManager.Draw(RavenDive, PlayerHelper.AllPlayers);
            }));
            ActionQueue.Enqueue((new HashSet<uint> { 9905u }, delegate
            {
                DrawManager.Draw(MeteorStream, PlayerHelper.AllPlayers);
            }));
            break;
        case 6504:
            DrawManager.Draw(IronChariot, nael);
            ActionQueue.Enqueue((new HashSet<uint> { 9915u }, delegate
            {
                SimpleLockon.ShareLockon((IGameObject?)Svc.Objects.LocalPlayer);
            }));
            DrawQueue.Enqueue((new HashSet<uint> { 9915u }, new(IGameObject, DrawElement[])[1] { ((IGameObject)Svc.Objects.LocalPlayer, new DrawElement[1] { ThermionicBeam }) }));
            ActionQueue.Enqueue((new HashSet<uint> { 9917u }, delegate
            {
                DrawManager.Draw(RavenDive, PlayerHelper.AllPlayers);
            }));
            break;
        case 6505:
            DrawManager.Draw(IronChariot, nael);
            ActionQueue.Enqueue((new HashSet<uint> { 9915u }, delegate
            {
                DrawManager.Draw(RavenDive, PlayerHelper.AllPlayers);
            }));
            ActionQueue.Enqueue((new HashSet<uint> { 9918u }, delegate
            {
                SimpleLockon.ShareLockon((IGameObject?)Svc.Objects.LocalPlayer);
            }));
            DrawQueue.Enqueue((new HashSet<uint> { 9918u }, new(IGameObject, DrawElement[])[1] { ((IGameObject)Svc.Objects.LocalPlayer, new DrawElement[1] { ThermionicBeam }) }));
            break;
        case 6506:
            DrawManager.Draw(LunarDynamo, nael);
            ActionQueue.Enqueue((new HashSet<uint> { 9916u }, delegate
            {
                DrawManager.Draw(RavenDive, PlayerHelper.AllPlayers);
            }));
            ActionQueue.Enqueue((new HashSet<uint> { 9918u }, delegate
            {
                SimpleLockon.ShareLockon((IGameObject?)Svc.Objects.LocalPlayer);
            }));
            DrawQueue.Enqueue((new HashSet<uint> { 9918u }, new(IGameObject, DrawElement[])[1] { ((IGameObject)Svc.Objects.LocalPlayer, new DrawElement[1] { ThermionicBeam }) }));
            break;
        case 6507:
            DrawManager.Draw(LunarDynamo, nael);
            DrawQueue.Enqueue((new HashSet<uint> { 9916u }, new(IGameObject, DrawElement[])[1] { (nael, new DrawElement[1] { IronChariot }) }));
            ActionQueue.Enqueue((new HashSet<uint> { 9915u }, delegate
            {
                DrawManager.Draw(RavenDive, PlayerHelper.AllPlayers);
            }));
            break;
        case 6498:
        case 6499:
            break;
        }
    }
}
