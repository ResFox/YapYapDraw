using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.SugarRiot;

public class SprayPain : ISpecialAction
{
	public override string Name => "Spray Pain";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42603u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(5);

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement element = new DrawElement
		{
			drawAvfx = "m0347_sircle_01m1",
			radiusX = 10f,
			radiusZ = 10f,
			delayDrawTime = 7000f
		};
		aoes.Add(DrawManager.Draw(element));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}
