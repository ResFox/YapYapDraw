using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.TheMesoTerminal;

public class SterileSphere : ISpecialAction
{
    public override string Name => "Sterile Sphere";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnEnvControl(byte index, uint state)
    {
        if (state != 131073)
        {
            return;
        }
        int radius;
        switch (index)
        {
        case 11:
        case 12:
        case 13:
        case 14:
            radius = 15;
            break;
        case 15:
        case 16:
        case 17:
        case 18:
            radius = 8;
            break;
        default:
            radius = 0;
            break;
        }
        if (radius != 0)
        {
            Vector3 pos;
            switch (index)
            {
            case 11:
            case 15:
                pos = new Vector3(260f, -582.5f, 2f);
                break;
            case 12:
            case 16:
                pos = new Vector3(280f, -582.5f, 2f);
                break;
            case 13:
            case 17:
                pos = new Vector3(260f, -582.5f, 22f);
                break;
            default:
                pos = new Vector3(280f, -582.5f, 22f);
                break;
            }
            SimpleElement.Circle(pos, radius, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 43805u, 43806u }
            });
        }
    }
}
