using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.JeunoArc1;

public class RakshasaSevenStarFist : ISpecialAction
{
    public override string Name => "Rakshasa Seven Star Fist";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40950u, 40951u, 40952u };

    public override void OnActionCast(ActorCastInfo info)
    {
        float radiusZ = info.ActionId switch
        {
            40950 => 12f, 
            40951 => 25f, 
            40952 => 38f, 
            _ => 0f, 
        };
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "e5d1_b1_kblaser_t1",
            radiusX = 1f,
            radiusZ = radiusZ,
            drawOnObject = true,
            KnockBackCheck = new KnockBackCheck
            {
                OriginPos = new Vector3(800f, 0f, 400f),
                Antiable = false
            },
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 40953u }
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
