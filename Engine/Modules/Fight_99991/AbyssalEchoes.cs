using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_99991;

public class AbyssalEchoes : ISpecialAction
{
    public override string Name => "Abyssal Echoes";

    public override HashSet<uint> ActionID => new HashSet<uint> { 35578u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 12f);
    }
}
