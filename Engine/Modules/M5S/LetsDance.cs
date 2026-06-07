using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M5S;

public class LetsDance : ISpecialAction
{
    public override string Name => "Let's Dance!";

    public override HashSet<uint> ActionID => new HashSet<uint> { 39901u, 41877u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public unsafe override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 334)
        {
            var address = (Character*)actorId.GameObject()!.Address;
            Angle refRotation = address->Timeline.ModelState switch
            {
                5 => 90.Degrees(), 
                7 => -90.Degrees(), 
                31 => 0.Degrees(), 
                32 => 180.Degrees(), 
                _ => 0.Degrees(), 
            };
            DrawElement element = new DrawElement
            {
                drawAvfx = "general02xf",
                Position = new Vector3(100f, 0f, 100f),
                drawOnObject = false,
                radiusX = 25f,
                radiusZ = 25f,
                refRotation = refRotation,
                refColor = new Vector4(1f, 1f, 1f, 2f),
                refTargetColor = new Vector4(1f, 1f, 1f, 2f),
                hitCounter = new HitCounter
                {
                    ActionID = ActionID,
                    TargetHitCount = 8
                }
            };
            aoes.Add(DrawManager.Draw(element));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
