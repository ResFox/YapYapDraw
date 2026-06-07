using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.GolbezEx;
public class PlasmaRay : ISpecialAction
{
    public uint Car = 1u;

    public override string Name => "Plasma Ray";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        if (GameObject.BaseId != 19000 || GameObject.Position.Y != 0f)
        {
            return;
        }
        int length = 30;
        if (Car == 2)
        {
            if (GameObject.Position.X == 92.5f)
            {
                length = 20;
            }
            else if (GameObject.Position.X == 107.5f)
            {
                length = 10;
            }
        }
        SimpleElement.RectangleMdl(GameObject, length, 2.5f, 0f, GameObject.Rotation.Radians(), 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 45671u, 45672u }
        });
    }

    public override void OnEnvControl(byte index, uint state)
    {
        switch (index)
        {
        case 4:
            if (state == 131073)
            {
                Car = 2u;
            }
            break;
        case 5:
            if (state == 131073)
            {
                Car = 3u;
            }
            break;
        }
    }

    public override void Reset()
    {
        Car = 1u;
        base.Reset();
    }
}
