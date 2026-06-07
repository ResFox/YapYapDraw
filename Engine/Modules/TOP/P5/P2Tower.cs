using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP.P5;

public class P2Tower : ISpecialAction
{
    private Dictionary<string, Vector2> P5TowerDirs = new Dictionary<string, Vector2>
    {
        {
            "N",
            new Vector2(100f, 100f)
        },
        {
            "S",
            new Vector2(100f, 100f)
        },
        {
            "E",
            new Vector2(100f, 100f)
        },
        {
            "W",
            new Vector2(100f, 100f)
        },
        {
            "NE",
            new Vector2(100f, 100f)
        },
        {
            "NW",
            new Vector2(100f, 100f)
        },
        {
            "SE",
            new Vector2(100f, 100f)
        },
        {
            "SW",
            new Vector2(100f, 100f)
        }
    };

    public override string Name => "P2 (tower)";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 32788u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 32788)
        {
            base.CanDraw = true;
            P5TowerDirs = new Dictionary<string, Vector2>
            {
                {
                    "N",
                    new Vector2(100f, 100f)
                },
                {
                    "S",
                    new Vector2(100f, 100f)
                },
                {
                    "E",
                    new Vector2(100f, 100f)
                },
                {
                    "W",
                    new Vector2(100f, 100f)
                },
                {
                    "NE",
                    new Vector2(100f, 100f)
                },
                {
                    "NW",
                    new Vector2(100f, 100f)
                },
                {
                    "SE",
                    new Vector2(100f, 100f)
                },
                {
                    "SW",
                    new Vector2(100f, 100f)
                }
            };
            Plugin.DebugChat("P5 P2 guide init");
        }
    }

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        uint baseId = GameObject.BaseId;
        bool isTower = baseId - 2013245 <= 1;
        if (!isTower || !base.CanDraw)
        {
            return;
        }
        base.CanDraw = false;
        new TimeHelper(100L, delegate
        {
            IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
            HeaderMarkerEnum mark = ((localPlayer != null) ? ((IGameObject)localPlayer).GameObjectId.Mark() : HeaderMarkerEnum.None);
            IEnumerable<IGameObject> towers = Svc.Objects.Where(o =>
            {
                uint baseId2 = o.BaseId;
                return baseId2 - 2013245 <= 1;
            });
            IGameObject markerObj = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 15720);
            float sumX = 0f;
            float sumZ = 0f;
            foreach (IGameObject item in towers)
            {
                sumX += item.Position.X;
                sumZ += item.Position.Z;
                Vector2 pos = new Vector2(item.Position.X, item.Position.Z);
                double bearing = VectorAngle(new Vector2(markerObj.Position.X, markerObj.Position.Z), pos);
                if (-5.0 < bearing && bearing < 5.0)
                {
                    P5TowerDirs["N"] = pos;
                }
                if (bearing > 175.0 || bearing < -175.0)
                {
                    P5TowerDirs["S"] = pos;
                }
                if (60.0 < bearing && bearing < 120.0)
                {
                    P5TowerDirs["E"] = pos;
                }
                if (-60.0 > bearing && bearing > -120.0)
                {
                    P5TowerDirs["W"] = pos;
                }
                if (15.0 < bearing && bearing < 50.0)
                {
                    P5TowerDirs["NE"] = pos;
                }
                if (-15.0 > bearing && bearing > -50.0)
                {
                    P5TowerDirs["NW"] = pos;
                }
                if (133.0 < bearing && bearing < 165.0)
                {
                    P5TowerDirs["SE"] = pos;
                }
                if (-133.0 > bearing && bearing > -165.0)
                {
                    P5TowerDirs["SW"] = pos;
                }
            }
            string layout = ((Vector2.Distance(new Vector2(sumX / (float)towers.Count(), sumZ / (float)towers.Count()), new Vector2(markerObj.Position.X, markerObj.Position.Z)) < 20f) ? "Reversed" : "Normal");
            Vector2 safeSpot = new Vector2(100f, 100f);
            if (towers.Count() == 5)
            {
                if (layout == "Normal")
                {
                    switch (mark)
                    {
                    case HeaderMarkerEnum.Attack1:
                    case HeaderMarkerEnum.Chain1:
                        safeSpot = P5TowerDirs["N"];
                        break;
                    case HeaderMarkerEnum.Attack3:
                    case HeaderMarkerEnum.Attack4:
                        safeSpot = P5TowerDirs["SW"];
                        break;
                    case HeaderMarkerEnum.Chain3:
                    case HeaderMarkerEnum.Circle:
                        safeSpot = P5TowerDirs["SE"];
                        break;
                    case HeaderMarkerEnum.Attack2:
                        safeSpot = P5TowerDirs["W"];
                        break;
                    case HeaderMarkerEnum.Chain2:
                        safeSpot = P5TowerDirs["E"];
                        break;
                    }
                }
                else
                {
                    switch (mark)
                    {
                    case HeaderMarkerEnum.Attack4:
                    case HeaderMarkerEnum.Circle:
                        safeSpot = P5TowerDirs["S"];
                        break;
                    case HeaderMarkerEnum.Attack1:
                    case HeaderMarkerEnum.Attack2:
                        safeSpot = P5TowerDirs["NW"];
                        break;
                    case HeaderMarkerEnum.Chain1:
                    case HeaderMarkerEnum.Chain2:
                        safeSpot = P5TowerDirs["NE"];
                        break;
                    case HeaderMarkerEnum.Attack3:
                        safeSpot = P5TowerDirs["W"];
                        break;
                    case HeaderMarkerEnum.Chain3:
                        safeSpot = P5TowerDirs["E"];
                        break;
                    }
                }
            }
            else if (layout == "Normal")
            {
                switch (mark)
                {
                case HeaderMarkerEnum.Chain1:
                    safeSpot = P5TowerDirs["NW"];
                    break;
                case HeaderMarkerEnum.Attack1:
                    safeSpot = P5TowerDirs["NE"];
                    break;
                case HeaderMarkerEnum.Attack3:
                    safeSpot = P5TowerDirs["SW"];
                    break;
                case HeaderMarkerEnum.Chain3:
                    safeSpot = P5TowerDirs["SE"];
                    break;
                case HeaderMarkerEnum.Attack2:
                case HeaderMarkerEnum.Attack4:
                    safeSpot = P5TowerDirs["W"];
                    break;
                case HeaderMarkerEnum.Chain2:
                case HeaderMarkerEnum.Circle:
                    safeSpot = P5TowerDirs["E"];
                    break;
                }
            }
            else
            {
                switch (mark)
                {
                case HeaderMarkerEnum.Attack2:
                    safeSpot = P5TowerDirs["NW"];
                    break;
                case HeaderMarkerEnum.Chain2:
                    safeSpot = P5TowerDirs["NE"];
                    break;
                case HeaderMarkerEnum.Circle:
                    safeSpot = P5TowerDirs["SW"];
                    break;
                case HeaderMarkerEnum.Attack4:
                    safeSpot = P5TowerDirs["SE"];
                    break;
                case HeaderMarkerEnum.Attack1:
                case HeaderMarkerEnum.Attack3:
                    safeSpot = P5TowerDirs["W"];
                    break;
                case HeaderMarkerEnum.Chain1:
                case HeaderMarkerEnum.Chain3:
                    safeSpot = P5TowerDirs["E"];
                    break;
                }
            }
            if (safeSpot.X != 100f || safeSpot.Y != 100f)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "share_trap01k1",
                    Position = new Vector3(safeSpot.X, 0f, safeSpot.Y),
                    drawOnObject = false,
                    radiusX = 3f,
                    radiusY = 5f,
                    radiusZ = 3f,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 31492u }
                    }
                }, markerObj);
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "e5d1_b1_kblaser_t1",
                    radiusX = 1.5f,
                    drawOnObject = true,
                    targetPosition = new Vector3(safeSpot.X, 0f, safeSpot.Y),
                    endToTarget = true,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 31492u }
                    }
                }, (IGameObject?)Svc.Objects.LocalPlayer);
                Plugin.DebugChat("P5 P2 tower guide");
            }
        });
    }

    private static double VectorAngle(Vector2 mainVector, Vector2 minorVector, int type = 0)
    {
        float mainX;
        float mainY;
        float minorX;
        float minorY;
        if (type != 1)
        {
            mainX = mainVector.X - 100f;
            mainY = 0f - (mainVector.Y - 100f);
            minorX = minorVector.X - 100f;
            minorY = 0f - (minorVector.Y - 100f);
        }
        else
        {
            mainX = mainVector.X;
            mainY = mainVector.Y;
            minorX = minorVector.X;
            minorY = minorVector.Y;
        }
        float dot = mainX * minorX + mainY * minorY;
        float mainLen = MathF.Sqrt(mainX * mainX + mainY * mainY);
        float minorLen = MathF.Sqrt(minorX * minorX + minorY * minorY);
        double angle = Math.Acos(Math.Clamp(dot / (mainLen * minorLen), -1.0, 1.0));
        double sign = ((!(mainX * minorY - minorX * mainY > 0f)) ? 1 : (-1));
        return angle * sign * (180.0 / Math.PI);
    }
}
