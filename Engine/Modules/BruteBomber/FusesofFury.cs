using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.BruteBomber;

public class FusesofFury : ISpecialAction
{
	public override string Name => "Fuses of Fury";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37816u, 37817u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 37816)
		{
			SimpleElement.Circle(info, 8f);
		}
		if (info.ActionId == 37817)
		{
			SimpleElement.Circle(info, 8f, 5000f);
		}
	}
}
