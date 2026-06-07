using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.SpheneDarkEx;
public class AzureSoul : ISpecialAction
{
    public List<string> Hints = new List<string>(4);

    public override string Name => "Azure Soul";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
        switch (icon)
        {
        case 604u:
            Hints.Add("Circle");
            break;
        case 605u:
            Hints.Add("Donut");
            break;
        case 606u:
            Hints.Add("Sides");
            break;
        case 607u:
            Hints.Add("Middle");
            break;
        }
    }

    public override void OnActorModelStateChange(IGameObject obj, byte modelState, byte animState1, byte animState2)
    {
        if (Hints.Count != 4)
        {
            return;
        }
        int offset = modelState switch
        {
            21 => 0, 
            147 => 1, 
            65 => 2, 
            22 => 3, 
            _ => -1, 
        };
        if (offset == -1)
        {
            return;
        }
        Utils.RotateList(Hints, offset);
        for (int i = 0; i < 4; i++)
        {
            Vector3 position = obj.Position;
            switch (Hints[i])
            {
            case "Sides":
            {
                Vector3 pos = new Vector3(88f, 0f, 85f);
                float castTime = ((i == 0) ? 12400 : 2800);
                float delay = ((i != 0) ? (12400 + (i - 1) * 2800) : 0);
                SimpleElement.Rectangle(pos, 100f, 6f, 0f, default, castTime, delay);
                Vector3 pos2 = new Vector3(112f, 0f, 85f);
                delay = ((i == 0) ? 12400 : 2800);
                castTime = ((i != 0) ? (12400 + (i - 1) * 2800) : 0);
                SimpleElement.Rectangle(pos2, 100f, 6f, 0f, default, delay, castTime);
                break;
            }
            case "Circle":
                SimpleElement.Circle(position, 20f, (i == 0) ? 12400 : 2800, (i != 0) ? (12400 + (i - 1) * 2800) : 0);
                break;
            case "Donut":
                SimpleElement.Donut(position, 16f, 60f, (i == 0) ? 12400 : 2800, (i != 0) ? (12400 + (i - 1) * 2800) : 0);
                break;
            case "Middle":
            {
                float castTime = ((i == 0) ? 12400 : 2800);
                float delay = ((i != 0) ? (12400 + (i - 1) * 2800) : 0);
                SimpleElement.Rectangle(position, 100f, 6f, 0f, default, castTime, delay);
                break;
            }
            }
        }
        Hints.Clear();
    }

    public override void Reset()
    {
        Hints.Clear();
        base.Reset();
    }
}
