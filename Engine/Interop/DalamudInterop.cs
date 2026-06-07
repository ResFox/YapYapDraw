using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace YapYapDraw.Engine.Interop.ActionEffect
{
    public struct TargetEffect
    {
        public uint TargetID;
    }
}

namespace YapYapDraw.Engine.Interop
{
    internal static class Svc
    {
        public static IDataManager            Data            => YapYapDraw.Plugin.DataManager;
        public static IObjectTable            Objects         => YapYapDraw.Plugin.ObjectTable;
        public static IPluginLog              Log             => YapYapDraw.Plugin.Log;
        public static ISigScanner            SigScanner      => YapYapDraw.Plugin.SigScanner;
        public static IGameInteropProvider    Hook            => YapYapDraw.Plugin.GameInterop;
        public static IClientState            ClientState     => YapYapDraw.Plugin.ClientState;
        public static IFramework              Framework       => YapYapDraw.Plugin.Framework;
        public static ICondition              Condition       => YapYapDraw.Plugin.Condition;
        public static IDalamudPluginInterface PluginInterface => YapYapDraw.Plugin.PluginInterface;
        public static IAddonLifecycle         AddonLifecycle  => YapYapDraw.Plugin.AddonLifecycle;
    }

    internal static class InteropHelpers
    {
        public static void Log(this Exception e)
        {
            try { YapYapDraw.Plugin.Log?.Error(e, "[YapYapDraw] " + e.Message); } catch { }
        }
    }
}

namespace YapYapDraw.Engine.Interop.Ui
{
    internal static class ImGuiUtil
    {
        public static bool IconButton(FontAwesomeIcon icon, string tooltip)
        {
            var clicked = Dalamud.Bindings.ImGui.ImGui.Button($"{(char)icon}###{tooltip}");
            if (Dalamud.Bindings.ImGui.ImGui.IsItemHovered())
                Dalamud.Bindings.ImGui.ImGui.SetTooltip(tooltip);
            return clicked;
        }
    }
}

namespace YapYapDraw.Engine.Interop.Game
{
    public enum CombatRole : byte
    {
        NonCombat,
        Tank,
        Healer,
        DPS,
    }

    internal static class ObjectFunctions
    {
        public static CombatRole GetRole(this ICharacter chara)
        {
            var role = chara.ClassJob.ValueNullable?.Role ?? 0;
            return role switch
            {
                1 => CombatRole.Tank,
                4 => CombatRole.Healer,
                2 or 3 => CombatRole.DPS,
                _ => CombatRole.NonCombat,
            };
        }

        public static unsafe bool IsCharacterVisible(this ICharacter chara)
        {
            var addr = chara.Address;
            if (addr == IntPtr.Zero) return false;
            var c = (Character*)addr;
            return c->GameObject.RenderFlags == 0;
        }
    }

    internal static class CharacterFunctions
    {
        public static int GetTransformationID(this ICharacter chara) => 0;

        public static bool IsHostile(this IGameObject obj)
            => obj is IBattleChara bc && bc.IsHostile();
    }

    internal static class GameObjectExtensions
    {
        public static unsafe Character* Struct(this IGameObject obj)
            => (Character*)obj.Address;
    }

    internal static class Player
    {
        public static IGameObject Object => YapYapDraw.Plugin.ObjectTable.LocalPlayer!;
    }
}
