using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.DancingGreen;

public class LetsDance : ISpecialAction
{
	public override string Name => "Let's Dance";

	public override HashSet<uint> ActionID => new HashSet<uint> { 39900u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public unsafe override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 334)
		{
			Character* address = (Character*)actorId.GameObject().Address;
			Angle refRotation = address->Timeline.ModelState switch
			{
				5 => 90.Degrees(),
				_ => -90.Degrees()
			};
			aoes.Add(SimpleElement.Rectangle(new Vector3(100f, 0f, 100f), 20f, 20f, 0f, refRotation, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 39900u },
				TargetHitCount = 8
			}));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}
