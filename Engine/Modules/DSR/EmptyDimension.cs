using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class EmptyDimension : ISpecialAction
{
    public override string Name => "Empty Dimension";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25306u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        if (source != null)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "customDonut",
                refRadian = 3f / 35f,
                radiusX = 70f,
                radiusZ = 70f,
                drawOnObject = true,
                refColor = Vector4.One,
                refTargetColor = Vector4.One,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 25306u }
                }
            }, source);
        }
    }
}
