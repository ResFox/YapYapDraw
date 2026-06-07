using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class ApocalypseGround : ISpecialAction
{
    public override string Name => "Apocalypse (ground)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnEnvControl(byte index, uint state)
    {
        if (index == 41)
        {
            switch (state)
            {
            case 2097168u:
                base.CanDraw = true;
                base.NumCasts = 0;
                break;
            case 524289u:
                base.CanDraw = false;
                break;
            }
        }
    }

    public override void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
    {
        if (base.CanDraw)
        {
            base.NumCasts++;
            IGameObject actor = actorID.GameObject();
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 9f,
                radiusZ = 9f,
                destroyTime = ((base.NumCasts >= 13) ? 2000 : ((base.NumCasts < 5) ? 8200 : 10200)),
                delayDrawTime = ((base.NumCasts >= 13) ? 9200 : ((base.NumCasts < 5) ? 3000 : 1000))
            }, actor);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "yazirushi1o0c",
                radiusX = 5f,
                radiusZ = 5f,
                destroyTime = ((base.NumCasts >= 13) ? 2000 : ((base.NumCasts < 5) ? 8200 : 10200)),
                delayDrawTime = ((base.NumCasts >= 13) ? 9200 : ((base.NumCasts < 5) ? 3000 : 1000))
            }, actor);
            if (p2 == 32 || p2 == 128)
            {
                Vector2 offset = new Vector2(actor.Position.X, actor.Position.Z) - new Vector2(100f, 100f);
                Vector2 rotatedPos = new Vector2(100f, 100f) + offset.RotationDegress(45f, p2 == 32);
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    Position = new Vector3(rotatedPos.X, 0f, rotatedPos.Y),
                    drawOnObject = false,
                    radiusX = 9f,
                    radiusZ = 9f,
                    delayDrawTime = 11200f,
                    destroyTime = 2000f
                }, actor);
            }
        }
    }
}
