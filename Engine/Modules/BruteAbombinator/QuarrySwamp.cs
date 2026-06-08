using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.BruteAbombinator;

public class QuarrySwamp : ISpecialAction
{
	public override string Name => "Quarry Swamp";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42285u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleLockon.EyeWarn(info.SourceId.GameObject());
	}
}
