using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M6S;

public class Highlightning : ISpecialAction
{
    private Vector3 lastPosition;

    public override string Name => "Highlightning";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 42651u };

    public override void Update()
    {
        if (aoes.Count == 0)
        {
            return;
        }
        IGameObject source = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18339);
        if (source != null)
        {
            Vector2 safeSpot;
            switch ((int)Angle.FromDirection(new WPos(source.Position) - new WPos(lastPosition)).Deg)
            {
            case 0:
                return;
            case -150:
            case -149:
            case -90:
                safeSpot = new Vector2(86.992f, 91.997f);
                break;
            case 90:
            case 146:
            case 147:
                safeSpot = new Vector2(114.977f, 91.997f);
                break;
            case -35:
            case -34:
            case -33:
            case -32:
            case 28:
            case 29:
                safeSpot = new Vector2(99.992f, 114.997f);
                break;
            default:
                safeSpot = default;
                break;
            }
            Vector2 spot = safeSpot;
            if (spot != default)
            {
                aoes[0].Position = new Vector3(spot.X, 0f, spot.Y);
            }
        }
    }

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        if (GameObject.BaseId == 18339)
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "general_1bxf",
                Position = GameObject.Position,
                drawOnObject = false,
                radiusX = 21f,
                radiusZ = 21f,
                refColor = Vector4.One,
                refTargetColor = Vector4.One,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 42651u },
                    TargetHitCount = 5
                }
            };
            aoes.Add(DrawManager.Draw(element));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (base.NumCasts++ == 5)
        {
            aoes.Clear();
        }
        lastPosition = info.Source.Position;
    }
}
