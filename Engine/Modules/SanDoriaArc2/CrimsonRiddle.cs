using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.SanDoriaArc2;

public class CrimsonRiddle : ISpecialAction
{
    public override string Name => "Crimson Riddle";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45045u, 45044u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 180);
    }
}
