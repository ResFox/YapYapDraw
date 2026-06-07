using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.ShishuVc;

public class MermaidDariaCrossingCurrent : ISpecialAction
{
    public override string Name => "Mermaid Daria Crossing Current";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
        if (icon != 20)
        {
            return;
        }
        IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
        if (TargetID != ((localPlayer != null) ? new ulong?(((IGameObject)localPlayer).GameObjectId) : ((ulong?)null)))
        {
            DrawElement obj = new DrawElement
            {
                drawAvfx = "general_x02f",
                radiusX = 4f,
                radiusZ = 36f,
                drawOnObject = false,
                fixRotation = true,
                PositionCustomAction = delegate
                {
                    Utils.GridSnapper gridSnapper = new Utils.GridSnapper
                    {
                        Center = new Vector3(375f, -29.5f, 530f),
                        Size = 40f,
                        GridCount = 5
                    };
                    return gridSnapper.Snap(Source.Position);
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 45860u }
                }
            };
            DrawManager.Draw(obj);
            obj.refRotation += 90.Degrees();
            DrawManager.Draw(obj);
        }
    }
}
