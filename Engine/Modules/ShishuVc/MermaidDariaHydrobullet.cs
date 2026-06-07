using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.ShishuVc;

public class MermaidDariaHydrobullet : ISpecialAction
{
    public override string Name => "Mermaid Daria Hydrobullet";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
        if (icon == 22)
        {
            IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
            if (TargetID != ((localPlayer != null) ? new ulong?(((IGameObject)localPlayer).GameObjectId) : ((ulong?)null)))
            {
                SimpleElement.Circle(Source, 15f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 45848u }
                });
            }
        }
    }
}
