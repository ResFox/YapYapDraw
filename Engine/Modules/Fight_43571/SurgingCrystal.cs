using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_43571;

public class SurgingCrystal : ISpecialAction
{
    private string[,] coordinates = new string[4, 4]
    {
        { "-15,-15", "-5,-15", "5,-15", "15,-15" },
        { "-15,-5", "-5,-5", "5,-5", "15,-5" },
        { "-15,5", "-5,5", "5,5", "15,5" },
        { "-15,15", "-5,15", "5,15", "15,15" }
    };

    private int count;

    public override string Name => "Surging Crystal";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 35496u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        IGameObject crystal = (from obj in (IEnumerable<IGameObject>)Svc.Objects
            where obj.BaseId == 16542
            orderby DistanceToTarget(obj, info.Source)
            select obj).FirstOrDefault();
        string cellKey = $"{Math.Round(crystal.Position.X)},{Math.Round(crystal.Position.Z)}";
        if (count == 1)
        {
            foreach (IGameObject item in Svc.Objects.Where((IGameObject obj) => obj.BaseId == 16542))
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general_x02f",
                    radiusX = 5f,
                    radiusZ = 100f,
                    drawOnObject = true,
                    destroyTime = 20000f
                }, item);
            }
            count++;
            return;
        }
        for (int row = 0; row < coordinates.GetLength(0); row++)
        {
            for (int col = 0; col < coordinates.GetLength(1); col++)
            {
                if (coordinates[row, col] == cellKey)
                {
                    double angle = Math.Floor(ConvertDegreesToRadians(crystal.Rotation));
                    Plugin.DebugLog(crystal.Rotation + " angle:" + angle);
                    if (angle == -1.0)
                    {
                        int nextCol = col + 1;
                        int prevCol = col - 1;
                        DrawManager.Draw(new DrawElement
                        {
                            drawAvfx = "general_x02f",
                            Position = new Vector3(Convert.ToInt32(coordinates.GetValue(0, nextCol).ToString().Split(",")[0]), 0f, Convert.ToInt32(coordinates.GetValue(0, nextCol).ToString().Split(",")[1])),
                            drawOnObject = false,
                            radiusX = 5f,
                            radiusZ = 5f,
                            destroyTime = 35000f
                        }, crystal);
                        DrawManager.Draw(new DrawElement
                        {
                            drawAvfx = "general_x02f",
                            Position = new Vector3(Convert.ToInt32(coordinates.GetValue(3, nextCol).ToString().Split(",")[0]), 0f, Convert.ToInt32(coordinates.GetValue(3, nextCol).ToString().Split(",")[1])),
                            drawOnObject = false,
                            radiusX = 5f,
                            radiusZ = 5f,
                            destroyTime = 35000f
                        }, crystal);
                        DrawManager.Draw(new DrawElement
                        {
                            drawAvfx = "general_x02f",
                            Position = new Vector3(Convert.ToInt32(coordinates.GetValue(0, prevCol).ToString().Split(",")[0]), 0f, Convert.ToInt32(coordinates.GetValue(0, prevCol).ToString().Split(",")[1])),
                            drawOnObject = false,
                            radiusX = 5f,
                            radiusZ = 5f,
                            destroyTime = 35000f
                        }, crystal);
                        DrawManager.Draw(new DrawElement
                        {
                            drawAvfx = "general_x02f",
                            Position = new Vector3(Convert.ToInt32(coordinates.GetValue(3, prevCol).ToString().Split(",")[0]), 0f, Convert.ToInt32(coordinates.GetValue(3, prevCol).ToString().Split(",")[1])),
                            drawOnObject = false,
                            radiusX = 5f,
                            radiusZ = 5f,
                            destroyTime = 35000f
                        }, crystal);
                    }
                    else if (angle == -91.0)
                    {
                        int nextRow = row + 1;
                        int prevRow = row - 1;
                        DrawManager.Draw(new DrawElement
                        {
                            drawAvfx = "general_x02f",
                            Position = new Vector3(Convert.ToInt32(coordinates.GetValue(nextRow, 0).ToString().Split(",")[0]), 0f, Convert.ToInt32(coordinates.GetValue(nextRow, 0).ToString().Split(",")[1])),
                            drawOnObject = false,
                            radiusX = 5f,
                            radiusZ = 5f,
                            destroyTime = 35000f
                        }, crystal);
                        DrawManager.Draw(new DrawElement
                        {
                            drawAvfx = "general_x02f",
                            Position = new Vector3(Convert.ToInt32(coordinates.GetValue(nextRow, 3).ToString().Split(",")[0]), 0f, Convert.ToInt32(coordinates.GetValue(nextRow, 3).ToString().Split(",")[1])),
                            drawOnObject = false,
                            radiusX = 5f,
                            radiusZ = 5f,
                            destroyTime = 35000f
                        }, crystal);
                        DrawManager.Draw(new DrawElement
                        {
                            drawAvfx = "general_x02f",
                            Position = new Vector3(Convert.ToInt32(coordinates.GetValue(prevRow, 0).ToString().Split(",")[0]), 0f, Convert.ToInt32(coordinates.GetValue(prevRow, 0).ToString().Split(",")[1])),
                            drawOnObject = false,
                            radiusX = 5f,
                            radiusZ = 5f,
                            destroyTime = 35000f
                        }, crystal);
                        DrawManager.Draw(new DrawElement
                        {
                            drawAvfx = "general_x02f",
                            Position = new Vector3(Convert.ToInt32(coordinates.GetValue(prevRow, 3).ToString().Split(",")[0]), 0f, Convert.ToInt32(coordinates.GetValue(prevRow, 3).ToString().Split(",")[1])),
                            drawOnObject = false,
                            radiusX = 5f,
                            radiusZ = 5f,
                            destroyTime = 35000f
                        }, crystal);
                    }
                }
            }
        }
        count++;
    }

    public override void Reset()
    {
        count = 0;
    }

    public static double ConvertDegreesToRadians(double degrees)
    {
        return degrees * (180.0 / Math.PI);
    }

    public static float DistanceToTarget(IGameObject obj, IGameObject target)
    {
        return Vector3.Distance(target.Position, obj.Position);
    }
}
