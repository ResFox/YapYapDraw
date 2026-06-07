using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class AscalonsMight : ISpecialAction
{
    private bool canDraw = true;

    public override string Name => "Ascalon's Might";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25541u, 25543u, 25545u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 25541)
        {
            canDraw = true;
        }
        uint actionId = info.ActionId;
        if (actionId == 25543 || actionId == 25545)
        {
            IGameObject dragon = Svc.Objects.FirstOrDefault(o =>
            {
                uint baseId = o.BaseId;
                return baseId == 12604 || baseId == 12611;
            });
            if (dragon != null && dragon.IsTargetable && canDraw)
            {
                canDraw = false;
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "gl_fan090_1bf",
                    radiusX = 50f,
                    radiusZ = 50f,
                    drawOnObject = true,
                    alwaysFaceCurrentTarget = true,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 25541u },
                        TargetHitCount = 3
                    }
                }, dragon);
            }
        }
    }

    public override void Reset()
    {
        canDraw = true;
        base.Reset();
    }
}
