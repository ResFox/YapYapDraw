using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.HoneyBLovely;

public class HoneyBeeline : ISpecialAction
{
	public override string Name => "Honey Beeline";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39737u, 39739u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 30f, 7f, 30f);
	}
}
