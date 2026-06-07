using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M8S;

public class ChampionsCircuit : ISpecialAction
{
    private enum Mechanic
    {
        None,
        Clockwise,
        Counterclockwise
    }

    private static readonly HashSet<uint> ChampionsCircuitFirst = new HashSet<uint> { 42105u, 42106u, 42107u, 42108u, 42109u };

    private static readonly HashSet<uint> ChampionsCircuitRest = new HashSet<uint> { 42110u, 42111u, 42112u, 42113u, 42114u };

    private Mechanic mechanic;

    public override string Name => "Champion's Circuit";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID
    {
        get
        {
            HashSet<uint> ids = new HashSet<uint>();
            foreach (uint id in ChampionsCircuitFirst)
            {
                ids.Add(id);
            }
            foreach (uint id in ChampionsCircuitRest)
            {
                ids.Add(id);
            }
            return ids;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (ChampionsCircuitFirst.Contains(info.ActionId) && mechanic != Mechanic.None)
        {
            Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow(info.ActionId);
            Omen value = ((Action)(row)).Omen.Value;
            ReadOnlySeString path = ((Omen)(value)).Path;
            string drawAvfx = ((ReadOnlySeString)(path)).ExtractText();
            DrawElement drawElement = new DrawElement();
            drawElement.drawAvfx = drawAvfx;
            DrawElement drawElement2 = drawElement;
            drawElement2.radiusX = info.ActionId switch
            {
                42105u => 6, 
                42106u => 13, 
                42107u => 28, 
                42108u => 22, 
                42109u => 28, 
                _ => 0, 
            };
            DrawElement drawElement3 = drawElement;
            drawElement3.radiusZ = info.ActionId switch
            {
                42105u => 30, 
                42106u => 13, 
                42107u => 28, 
                42108u => 22, 
                42109u => 28, 
                _ => 0, 
            };
            drawElement.drawOnObject = true;
            drawElement.refRotation = info.Rotation + ((mechanic == Mechanic.Clockwise) ? (-72.Degrees()) : 72.Degrees());
            drawElement.fixRotation = true;
            DrawElement drawElement4 = drawElement;
            HitCounter hitCounter = new HitCounter();
            HitCounter hitCounter2 = hitCounter;
            hitCounter2.ActionID = info.ActionId switch
            {
                42105u => new HashSet<uint> { 42110u }, 
                42106u => new HashSet<uint> { 42111u }, 
                42107u => new HashSet<uint> { 42112u }, 
                42108u => new HashSet<uint> { 42113u }, 
                42109u => new HashSet<uint> { 42114u }, 
                _ => new HashSet<uint> { 0u }, 
            };
            hitCounter.TargetHitCount = 4;
            drawElement4.hitCounter = hitCounter;
            DrawElement drawElement5 = drawElement;
            if (info.ActionId == 42106)
            {
                Vector2 offset = new Vector2(info.Source.Position.X, info.Source.Position.Z) - new Vector2(100f, 100f);
                Vector2 rotated = new Vector2(100f, 100f) + offset.RotationDegress(72f, mechanic == Mechanic.Clockwise);
                drawElement5.Position = new Vector3(rotated.X, -150f, rotated.Y);
                drawElement5.drawOnObject = false;
            }
            aoes.Add(DrawManager.Draw(drawElement5, info.Source));
        }
        else
        {
            if (!ChampionsCircuitRest.Contains(info.ActionId) || mechanic == Mechanic.None)
            {
                return;
            }
            base.NumCasts++;
            if (base.NumCasts % 5 != 0)
            {
                return;
            }
            int count = aoes.Count;
            for (int i = 0; i < count; i++)
            {
                StaticVfx aoe = aoes[i];
                if (aoe.Owner == null)
                {
                    Vector2 offset2 = new Vector2(aoe.Position.X, aoe.Position.Z) - new Vector2(100f, 100f);
                    Vector2 rotated = new Vector2(100f, 100f) + offset2.RotationDegress(72f, mechanic == Mechanic.Clockwise);
                    aoe.Position = new Vector3(rotated.X, -150f, rotated.Y);
                }
                aoe.Rotation += ((mechanic == Mechanic.Clockwise) ? (-72.Degrees()) : 72.Degrees());
            }
        }
    }

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        bool isMarker = icon - 501 <= 1;
        if (isMarker && target.BaseId == 18222)
        {
            if (icon == 501)
            {
                mechanic = Mechanic.Clockwise;
            }
            else
            {
                mechanic = Mechanic.Counterclockwise;
            }
        }
    }

    public override void Reset()
    {
        mechanic = Mechanic.None;
        base.Reset();
    }
}
