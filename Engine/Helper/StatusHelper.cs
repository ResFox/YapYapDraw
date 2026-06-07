using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Engine.Helper;

public static class StatusHelper
{
    public static bool HasStatus(this IGameObject? obj, uint StatusId)
    {
        if (obj is IBattleChara chara)
        {
            return chara.StatusList.Any(s => s.StatusId == StatusId);
        }
        return false;
    }

    public static bool HasStatus(this IGameObject? obj, uint[] StatusIds)
    {
        if (obj is IBattleChara chara)
        {
            return chara.StatusList.Any(s => StatusIds.Contains(s.StatusId));
        }
        return false;
    }

    public static bool GetStack(uint GameObjectId, uint StatusId, out int stack)
    {
        stack = 0;
        IGameObject obj = GameObjectId.GameObject();
        if (obj is IBattleChara chara)
        {
            var status = chara.StatusList.FirstOrDefault(s => s.StatusId == StatusId);
            if (status == null)
            {
                return false;
            }
            stack = status.Param;
            return true;
        }
        return false;
    }

    public static bool GetStack(this IGameObject GameObject, uint StatusId, out int stack)
    {
        stack = 0;
        if (GameObject is IBattleChara chara)
        {
            var status = chara.StatusList.FirstOrDefault(s => s.StatusId == StatusId);
            if (status == null)
            {
                return false;
            }
            stack = status.Param;
            return true;
        }
        return false;
    }

    public static bool GetParam(uint GameObjectId, uint StatusId, out int param)
    {
        param = 0;
        IGameObject obj = GameObjectId.GameObject();
        if (obj is IBattleChara chara)
        {
            var status = chara.StatusList.FirstOrDefault(s => s.StatusId == StatusId);
            if (status == null)
            {
                return false;
            }
            param = status.Param;
            return true;
        }
        return false;
    }

    public static bool GetParam(this IGameObject GameObject, uint StatusId, out int param)
    {
        param = 0;
        if (GameObject is IBattleChara chara)
        {
            var status = chara.StatusList.FirstOrDefault(s => s.StatusId == StatusId);
            if (status == null)
            {
                return false;
            }
            param = status.Param;
            return true;
        }
        return false;
    }

    public static bool GetStatusRemainingTime(uint GameObjectId, uint StatusId, out float time)
    {
        time = 0f;
        IGameObject obj = GameObjectId.GameObject();
        if (obj is IBattleChara chara)
        {
            var status = chara.StatusList.FirstOrDefault(s => s.StatusId == StatusId);
            if (status == null)
            {
                return false;
            }
            time = status.RemainingTime;
            return true;
        }
        return false;
    }

    public static bool GetStatusRemainingTime(this IGameObject GameObject, uint StatusId, out float time)
    {
        time = 0f;
        if (GameObject is IBattleChara chara)
        {
            var status = chara.StatusList.FirstOrDefault(s => s.StatusId == StatusId);
            if (status == null)
            {
                return false;
            }
            time = status.RemainingTime;
            return true;
        }
        return false;
    }

    public static bool KnockBackAnti(this IGameObject? obj)
    {
        if (obj is IBattleChara chara)
        {
            return chara.StatusList.Any(s => s.StatusId == 160 || s.StatusId == 1209 || s.StatusId == 2663);
        }
        return false;
    }
}
