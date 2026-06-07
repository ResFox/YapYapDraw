using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_33687;

public class Aethertithe : ISpecialAction
{
    public override string Name => "Aethertithe";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    private static HashSet<uint> CastEnd => new HashSet<uint> { 36657u, 36658u, 36659u };

    public override void OnEnvControl(byte index, uint state)
    {
        if (index == 0)
        {
            IGameObject source = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 16832);
            switch (state)
            {
            case 67109120u:
                SimpleElement.Fan(new Vector3(source.Position.X, 0f, source.Position.Z), 100f, 70, -55.Degrees(), 3000f, 0f, new HitCounter
                {
                    ActionID = CastEnd
                });
                break;
            case 134217984u:
                SimpleElement.Fan(new Vector3(source.Position.X, 0f, source.Position.Z), 100f, 70, 0.Degrees(), 3000f, 0f, new HitCounter
                {
                    ActionID = CastEnd
                });
                break;
            case 268435712u:
                SimpleElement.Fan(new Vector3(source.Position.X, 0f, source.Position.Z), 100f, 70, 55.Degrees(), 3000f, 0f, new HitCounter
                {
                    ActionID = CastEnd
                });
                break;
            }
        }
    }
}
