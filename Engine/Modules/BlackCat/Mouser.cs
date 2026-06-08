using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.BlackCat;

public class Mouser : ISpecialAction
{
	public override string Name => "Mouser";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37653u, 39275u, 38053u };

	public override void OnActionCast(ActorCastInfo info)
	{
		ushort actionId = info.ActionId;
		if ((actionId != 37653 && actionId != 39275) || 1 == 0)
		{
			return;
		}
		base.NumCasts++;
		IGameObject val = info.SourceId.GameObject();
		if (val != null)
		{
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "customRect2",
				Position = val.Position,
				radiusX = 5f,
				radiusZ = 5f,
				fixRotation = true,
				drawOnObject = false,
				refColor = GroundOmen.enemyColor,
				refTargetColor = GroundOmen.enemyColor,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38053u },
					TargetHitCount = base.NumCasts
				}
			};
			if (info.ActionId == 39275)
			{
				drawElement.refColor = new Vector4(1f, 0f, 0f, 1f);
				drawElement.refTargetColor = new Vector4(1f, 0f, 0f, 1f);
			}
			DrawManager.Draw(drawElement, val);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 38053)
		{
			base.NumCasts = 0;
		}
	}
}
