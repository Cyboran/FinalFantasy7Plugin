using System.IO;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface.Internal;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.Configuration
{
    public partial class Defaults
    {
        public const float PortraitX = 0;
        public const float PortraitY = 0;
        public const float PortraitScale = 1f;
        public const bool PortraitRedWhenDamaged = true;
        public const bool PortraitRedWhenDanger = true;
        public const string PortraitNormalImage = "";
        public const string PortraitHurtImage = "";
        public const string PortraitDangerImage = "";
        public const string PortraitCombatImage = "";
    }

    public partial class Settings
    {
        public float PortraitX { get; set; } = Defaults.PortraitX;
        public float PortraitY { get; set; } = Defaults.PortraitY;
        public float PortraitScale { get; set; } = Defaults.PortraitScale;
        public bool PortraitRedWhenDamaged { get; set; }  = Defaults.PortraitRedWhenDamaged;
        public bool PortraitRedWhenDanger { get; set; } = Defaults.PortraitRedWhenDanger;
        public string PortraitNormalImage { get; set; } = Defaults.PortraitNormalImage;
        public string PortraitHurtImage { get; set; } = Defaults.PortraitHurtImage;
        public string PortraitDangerImage { get; set; } = Defaults.PortraitDangerImage;
        public string PortraitCombatImage { get; set; } = Defaults.PortraitCombatImage;
    }
}

namespace KingdomHeartsPlugin.UIElements.Experience
{
    public static class Portrait
    {
        public static void SetAllPortraits()
        {
            SetPortraitNormal(FinalFantasy7Plugin.Ui.Configuration.PortraitNormalImage);
            SetPortraitHurt(FinalFantasy7Plugin.Ui.Configuration.PortraitHurtImage);
            SetPortraitDanger(FinalFantasy7Plugin.Ui.Configuration.PortraitDangerImage);
            SetPortraitCombat(FinalFantasy7Plugin.Ui.Configuration.PortraitCombatImage);
        }

        public static void SetPortraitNormal(string path)
        {
            PortraitNormal = GetTexture(path);
        }

        public static void SetPortraitHurt(string path)
        {
            PortraitHurt = GetTexture(path);
        }
        public static void SetPortraitDanger(string path)
        {
            PortraitDanger = GetTexture(path);
        }
        public static void SetPortraitCombat(string path)
        {
            PortraitCombat = GetTexture(path);
        }

        private static string GetTexture(string path)
        {
            if (path.IsNullOrEmpty() || !File.Exists(path)) 
            {
                FinalFantasy7Plugin.Pl.Warning($"Could not load image for portrait at: \"{path}\"");
                return "";
            }

            return path;
        }

        public static void Draw(float healthY)
        {
            if (FinalFantasy7Plugin.Cs.LocalPlayer == null) return;

            var drawList = ImGui.GetWindowDrawList();
            var drawPosition = new Vector2(FinalFantasy7Plugin.Ui.Configuration.PortraitX, FinalFantasy7Plugin.Ui.Configuration.PortraitY + healthY * FinalFantasy7Plugin.Ui.Configuration.Scale);
            var damagedAlpha = FinalFantasy7Plugin.Ui.Configuration.PortraitRedWhenDamaged ? FinalFantasy7Plugin.Ui.HealthFrame.DamagedHealthAlpha : 0;
            var realDamagedAlpha = FinalFantasy7Plugin.Ui.HealthFrame.DamagedHealthAlpha;
            var lowHealthAlpha = FinalFantasy7Plugin.Ui.Configuration.PortraitRedWhenDanger ? FinalFantasy7Plugin.Ui.HealthFrame.LowHealthAlpha : 0;
            var dangerStatus = FinalFantasy7Plugin.Cs.LocalPlayer.CurrentHp <= FinalFantasy7Plugin.Cs.LocalPlayer.MaxHp * (FinalFantasy7Plugin.Ui.Configuration.LowHpPercent / 100f);
            var portraitDangerAlpha = FinalFantasy7Plugin.Ui.Configuration.PortraitRedWhenDanger && dangerStatus ? 1 : 0;
            var inCombat = (FinalFantasy7Plugin.Cs.LocalPlayer.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat;

            //ImGuiAdditions.TextShadowedDrawList(drawList,24, $"{KingdomHeartsPlugin.Cs.LocalPlayer.StatusFlags}", ImGui.GetItemRectMin() + new Vector2(0,0), new Vector4(1, 1, 1, 1), new Vector4(0,0,0,1));

            if (realDamagedAlpha > 0.595f && PortraitHurt != "")
            {
                ImageDrawing.DrawImage(drawList, ImageDrawing.GetSharedTexture(PortraitHurt), FinalFantasy7Plugin.Ui.Configuration.PortraitScale, drawPosition, ImGui.GetColorU32(new Vector4(1 - lowHealthAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1)));
            }
            else if (dangerStatus && PortraitDanger != "")
            {
                ImageDrawing.DrawImage(drawList, ImageDrawing.GetSharedTexture(PortraitDanger), FinalFantasy7Plugin.Ui.Configuration.PortraitScale, drawPosition, ImGui.GetColorU32(new Vector4(1 - lowHealthAlpha, 1 - portraitDangerAlpha * 0.8f, 1 - portraitDangerAlpha * 0.8f, 1)));
            }
            else if (inCombat && PortraitCombat != "")
            {
                ImageDrawing.DrawImage(drawList, ImageDrawing.GetSharedTexture(PortraitCombat), FinalFantasy7Plugin.Ui.Configuration.PortraitScale, drawPosition, ImGui.GetColorU32(new Vector4(1 - lowHealthAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1)));
            }
            else if (PortraitNormal != "")
            {
                ImageDrawing.DrawImage(drawList, ImageDrawing.GetSharedTexture(PortraitNormal), FinalFantasy7Plugin.Ui.Configuration.PortraitScale, drawPosition, ImGui.GetColorU32(new Vector4(1 - lowHealthAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1)));
            }
        }

        public static void Dispose()
        {
        }


        private static string PortraitNormal { get; set; } = "";
        private static string PortraitHurt { get; set; } = "";
        private static string PortraitDanger { get; set; } = "";
        private static string PortraitCombat { get; set; } = "";
    }
}
