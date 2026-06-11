using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P3;

public class MahjongCharge : ISpecialAction
{
    private Vector3 _firstPos;
    private Vector3 _lastPos;
    private bool _clockwise;
    private readonly Dictionary<int, IGameObject> _players = new Dictionary<int, IGameObject>();

    public override string Name => "Mahjong Charge";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47843u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        NumCasts++;
        if (NumCasts == 1)
            _firstPos = info.Source.Position;
        if (NumCasts == 2)
            _lastPos = info.Source.Position;

        if (_firstPos != default && _lastPos != default)
            _clockwise = IsClockwise(new Vector3(100f, 0f, 100f), _firstPos, _lastPos);
    }

    public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
        switch (icon)
        {
            case 336u: _players[1] = Source; break;
            case 337u: _players[2] = Source; break;
            case 338u: _players[3] = Source; break;
            case 339u: _players[4] = Source; break;
            case 437u: _players[5] = Source; break;
            case 438u: _players[6] = Source; break;
            case 439u: _players[7] = Source; break;
            case 440u: _players[8] = Source; break;
        }

        if (_players.Count != 8)
            return;

        Vector3 center = new Vector3(100f, 0f, 100f);
        List<Vector3> lanes = new List<Vector3>(8) { _firstPos };
        float dir = _clockwise ? -1f : 1f;
        Vector3 spoke = _firstPos - center;
        for (int i = 1; i < 8; i++)
        {
            float angle = dir * i * (MathF.PI / 4f);
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            float x = spoke.X * cos - spoke.Z * sin;
            float z = spoke.X * sin + spoke.Z * cos;
            lanes.Add(center + new Vector3(x, 0f, z));
        }

        for (int j = 0; j < _players.Count; j++)
        {
            if (_players[j + 1] == (IGameObject?)Svc.Objects.LocalPlayer)
                continue;

            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                Position = lanes[j],
                drawOnObject = false,
                radiusX = 3f,
                radiusZ = 100f,
                target = _players[j + 1],
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47844u },
                    TargetHitCount = j + 1
                }
            });
        }
    }

    public override void Reset()
    {
        _players.Clear();
        base.Reset();
    }

    private static bool IsClockwise(Vector3 center, Vector3 a, Vector3 b)
        => Vector3.Cross(a - center, b - center).Y < 0f;
}
