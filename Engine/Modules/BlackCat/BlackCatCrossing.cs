using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.BlackCat;

public class BlackCatCrossing : ISpecialAction
{
	public override string Name => "Black Cat Crossing";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37649u, 37650u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 37649:
			SimpleElement.Fan(info, 60f, 45);
			break;
		case 37650:
			SimpleElement.Fan(info, 60f, 45, (info.CastTime - 3f) * 1000f);
			break;
		}
	}
}
