using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.QueenEternalEx;
public class IcicleTether : ISpecialAction
{
    public override string Name => "Icicle (tether)";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        bool isIcicleTether = Id == 1 || Id == 57;
        if (isIcicleTether && targetId == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
        {
            SimpleElement.RectangleToTarget(actorId.GameObject(), targetId.GameObject(), 80f, 2f, 3000f, new HitCounter
            {
                ActionID = new HashSet<uint> { 41015u, 41016u }
            });
            Vector2 vector = SafeSpot(actorId.GameObject());
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "share_trap01k1",
                Position = new Vector3(vector.X, 0f, vector.Y),
                drawOnObject = false,
                radiusX = 2f,
                radiusY = 5f,
                radiusZ = 2f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 41015u, 41016u }
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer);
        }
    }

    private static Vector2 SafeSpot(IGameObject source)
    {
        Vector2 center = new Vector2(100f, 100f);
        int xSign = ((!(source.Position.X > center.X)) ? 1 : (-1));
        float xDist = Math.Abs(source.Position.X - center.X);
        if (source.Position.Z > 110f)
        {
            bool isClose = xDist < 6f;
            return center + new Vector2(xSign * (isClose ? 15 : 10), -19f);
        }
        int zOffset = ((source.Position.Z < 96f) ? 9 : (-9));
        return center + new Vector2(xSign * 15, zOffset);
    }
}
