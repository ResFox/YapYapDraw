using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.QueenEternalEx;
public class Aethertithe : ISpecialAction
{
    public override string Name => "Aethertithe";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    private static HashSet<uint> CastEnd => new HashSet<uint> { 40974u, 40975u, 40976u };

    public override void OnEnvControl(byte index, uint state)
    {
        if (index == 0)
        {
            IGameObject boss = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18039);
            switch (state)
            {
            case 67109120u:
                SimpleElement.Fan(boss, 100f, 70, -55.Degrees(), 3000f, 0f, new HitCounter
                {
                    ActionID = CastEnd
                });
                break;
            case 134217984u:
            {
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = CastEnd
                };
                SimpleElement.Fan(boss, 100f, 70, default, 3000f, 0f, hitCounter);
                break;
            }
            case 268435712u:
                SimpleElement.Fan(boss, 100f, 70, 55.Degrees(), 3000f, 0f, new HitCounter
                {
                    ActionID = CastEnd
                });
                break;
            }
        }
    }
}
