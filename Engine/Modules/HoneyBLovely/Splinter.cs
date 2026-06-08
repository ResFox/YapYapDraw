using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.HoneyBLovely;

public class Splinter : ISpecialAction
{
	public override string Name => "Splinter";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37230u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info, 8f);
	}
}
