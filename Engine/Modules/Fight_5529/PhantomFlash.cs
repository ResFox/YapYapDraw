using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_5529;

public class PhantomFlash : ISpecialAction
{
    public override string Name => "Phantom Flash";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        if (GameObject.BaseId == 2011724)
        {
            SimpleElement.Circle(new Vector3(100f, 0f, 100f), 6f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 26061u }
            });
            for (int i = 1; i < 7; i++)
            {
                WPos wPos = WPos.RotateAroundOrigin(i * 60, new WPos(100f, 100f), new WPos(100f, 100f) + 17f * GameObject.Rotation.Radians().ToDirection());
                SimpleElement.Circle(new Vector3(wPos.X, 0f, wPos.Z), 6f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 26061u }
                });
            }
        }
        if (GameObject.BaseId == 2011725)
        {
            for (int j = 1; j < 4; j++)
            {
                WPos wPos2 = WPos.RotateAroundOrigin(-60 + j * 120, new WPos(100f, 100f), new WPos(100f, 100f) + 8f * GameObject.Rotation.Radians().ToDirection());
                SimpleElement.Circle(new Vector3(wPos2.X, 0f, wPos2.Z), 6f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 26061u }
                });
            }
        }
    }
}
