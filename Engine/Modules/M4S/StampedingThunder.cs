using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4S;

public class StampedingThunder : ISpecialAction
{
    public override string Name => "Stampeding Thunder";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38354u, 38355u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = "general02xf",
            drawOnObject = false,
            radiusX = 15f,
            radiusZ = 40f,
            refRotation = info.Source.Rotation.Radians(),
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 36399u }
            }
        };
        switch (info.ActionId)
        {
        case 38354u:
            SimpleElement.ShowText("Go right →→→", (TextGimmickHintStyle)0);
            drawElement.Position = new Vector3(info.Source.Position.X - 5f, 0f, info.Source.Position.Z);
            DrawManager.Draw(drawElement, (IGameObject?)Svc.Objects.LocalPlayer);
            break;
        case 38355u:
            SimpleElement.ShowText("←←← Go left", (TextGimmickHintStyle)0);
            drawElement.Position = new Vector3(info.Source.Position.X + 5f, 0f, info.Source.Position.Z);
            DrawManager.Draw(drawElement, (IGameObject?)Svc.Objects.LocalPlayer);
            break;
        }
    }
}
