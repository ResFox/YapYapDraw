using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UWU;

public class P3MistralSong : ISpecialAction
{
    public override string Name => "P3 Mistral Song";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11150u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 21.7f, 150);
    }
}
