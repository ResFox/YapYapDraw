using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.HoneyBLovely;

public class HoneyedBreeze : ISpecialAction
{
	public override string Name => "Honeyed Breeze";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 230)
		{
			IGameObject val = Svc.Objects.Where((IGameObject o) => o.BaseId == 16938 && o.IsTargetable).FirstOrDefault();
			if (val != null)
			{
				HitCounter hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 37224u }
				};
				SimpleElement.FanToTarget(val, target, 40f, 30, Follow: true, default, 0f, 3000f, hitCounter);
			}
		}
	}
}
