using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.SugarRiot;

public class Highlightning : ISpecialAction
{
	private Vector3 lastPosition;

	public override string Name => "Highlightning";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42599u };

	public override void Update()
	{
		if (aoes.Count == 0)
		{
			return;
		}
		IGameObject val = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18334);
		if (val == null)
		{
			aoes.ForEach(delegate(StaticVfx x)
			{
				x.Remove();
			});
			aoes.Clear();
			return;
		}
		Vector2 vector;
		switch ((int)Angle.FromDirection(new WDir(val.Position.X - lastPosition.X, val.Position.Z - lastPosition.Z)).Deg)
		{
		case 0:
			return;
		case -150:
		case -149:
		case -90:
			vector = new Vector2(86.992f, 91.997f);
			break;
		case 90:
		case 146:
		case 147:
			vector = new Vector2(114.977f, 91.997f);
			break;
		case -35:
		case -34:
		case -33:
		case -32:
		case 28:
		case 29:
			vector = new Vector2(99.992f, 114.997f);
			break;
		default:
			vector = default;
			break;
		}
		Vector2 vector2 = vector;
		if (vector2 != default)
		{
			aoes[0].Position = new Vector3(vector2.X, 0f, vector2.Y);
		}
	}

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 18334)
		{
			aoes.Add(SimpleElement.Circle(GameObject.Position, 21f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 42599u },
				TargetHitCount = 3
			}));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.NumCasts++;
		if (base.NumCasts == 3)
		{
			aoes.Clear();
			lastPosition = default;
			base.NumCasts = 0;
		}
		else
		{
			lastPosition = info.Source.Position;
		}
	}

	public override void Reset()
	{
		lastPosition = default;
		base.Reset();
	}
}
