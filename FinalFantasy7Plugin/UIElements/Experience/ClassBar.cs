using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using ImGuiScene;
using FinalFantasy7Plugin.Utilities;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using FinalFantasy7Plugin.Enums;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace FinalFantasy7Plugin.Configuration
{
    public partial class Defaults
    {
        public const float LevelTextX = 132;
        public const float LevelTextY = 81;
        public const float LevelTextSize = 32;
        public const TextAlignment LevelTextAlignment = TextAlignment.Center;

        public const float ClassIconX = 128;
        public const float ClassIconY = 150;
        public const float ClassIconScale = 1.0f;
    }

    public partial class Settings
    {
        public float LevelTextX { get; set; } = Defaults.LevelTextX;
        public float LevelTextY { get; set; } = Defaults.LevelTextY;
        public float LevelTextSize { get; set; } = Defaults.LevelTextSize;
        public TextAlignment LevelTextAlignment { get; set; } = Defaults.LevelTextAlignment;
        public float ClassIconX { get; set; } = Defaults.ClassIconX;
        public float ClassIconY { get; set; } = Defaults.ClassIconY;
        public float ClassIconScale { get; set; } = Defaults.ClassIconScale;
    }
}

namespace FinalFantasy7Plugin.UIElements.Experience
{
    public class ClassBar
    {
        private ISharedImmediateTexture _expBarSegmentTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(FinalFantasy7Plugin.TemplateLocation, @"Textures\Experience\ring_experience_segment.png"));
        }
        private ISharedImmediateTexture _expColorlessBarSegmentTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(FinalFantasy7Plugin.TemplateLocation, @"Textures\Experience\ring_experience_colorless_segment.png"));
        }
        private ISharedImmediateTexture _expBarBaseTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(FinalFantasy7Plugin.TemplateLocation, @"Textures\Experience\ring_experience_outline.png"));
        }

        unsafe
        private AddonExp* _addonExp;

        public ClassBar()
        {
            ExperienceRing = new Ring(Path.Combine(FinalFantasy7Plugin.TemplateLocation, @"Textures\Experience\ring_experience_segment.png"));
            ExperienceRingRest = new Ring(Path.Combine(FinalFantasy7Plugin.TemplateLocation, @"Textures\Experience\ring_experience_segment.png"), alpha: 0.25f);
            ExperienceRingGain = new Ring(Path.Combine(FinalFantasy7Plugin.TemplateLocation, @"Textures\Experience\ring_experience_colorless_segment.png"), 0.65f, 0.92f, 1.00f);
            ExperienceRingBg = new Ring(Path.Combine(FinalFantasy7Plugin.TemplateLocation, @"Textures\Experience\ring_experience_colorless_segment.png"), 0.07843f, 0.07843f, 0.0745f);
        }

        private unsafe void Update(IPlayerCharacter player)
        {
            _addonExp = (AddonExp*)FinalFantasy7Plugin.Gui.GetAddonByName("_Exp", 1);
            try
            {
                UpdateExperience(_addonExp->CurrentExp, _addonExp->RequiredExp, _addonExp->RestedExp, player.ClassJob.RowId, player.Level);
            }
            catch
            {
                try
                {
                    _addonExp = (AddonExp*)FinalFantasy7Plugin.Gui.GetAddonByName("_Exp", 1);
                }
                catch
                {
                    // ignored
                }
            }
        }
        private unsafe void UpdateExperience(uint exp, uint maxExp, uint rest, uint job, byte level)
        {
            if (_addonExp == null) return;

            if (LastLevel < level)
            {
                ExpTemp = 0;
                LastExp = 0;
            }

            if (LastJob != job)
            {
                ExpTemp = exp;
                LastExp = exp;
                ExpGainTime = 0;
            }

            if (LastExp > exp) ExpTemp = exp;

            if (LastExp < exp) GainExperience(LastExp);

            UpdateGainedExperience(exp);

            LastExp = exp;
            LastJob = job;
            LastLevel = level;
            Experience = exp;
            MaxExperience = maxExp;
            RestedBonusExperience = rest;
        }

        private void GainExperience(uint exp)
        {
            if (ExpGainTime <= 0)
            {
                ExpBeforeGain = exp;
                ExpTemp = exp;
            }

            ExpGainTime = 3f;
        }

        private void UpdateGainedExperience(uint exp)
        {
            if (ExpGainTime > 0)
            {
                ExpGainTime -= 1 * FinalFantasy7Plugin.UiSpeed;
            }
            else if (ExpTemp < exp)
            {
                ExpTemp += (exp - ExpBeforeGain) * FinalFantasy7Plugin.UiSpeed;
            }

            if (ExpTemp > exp)
                ExpTemp = exp;
        }

        public void Draw(IPlayerCharacter player, float healthY)
        {
            Update(player);
            var drawList = ImGui.GetWindowDrawList();

            int size = (int)Math.Ceiling(256 * FinalFantasy7Plugin.Ui.Configuration.Scale);
            var drawPosition = ImGui.GetItemRectMin() + new Vector2(0, (int)(healthY * FinalFantasy7Plugin.Ui.Configuration.Scale));

            if (FinalFantasy7Plugin.Ui.Configuration.ExpBarEnabled)
            {

                ExperienceRingBg?.Draw(drawList, 1, drawPosition, 4, FinalFantasy7Plugin.Ui.Configuration.Scale);

                ExperienceRingRest?.Draw(drawList, (Experience + RestedBonusExperience) / (float)MaxExperience, drawPosition, 4, FinalFantasy7Plugin.Ui.Configuration.Scale);

                ExperienceRingGain?.Draw(drawList, Experience / (float)MaxExperience, drawPosition, 4, FinalFantasy7Plugin.Ui.Configuration.Scale);

                ExperienceRing?.Draw(drawList, ExpTemp / MaxExperience, drawPosition, 4, FinalFantasy7Plugin.Ui.Configuration.Scale);

                drawList.PushClipRect(drawPosition, drawPosition + new Vector2(size, size));
                drawList.AddImage(_expBarBaseTexture.GetWrapOrEmpty().ImGuiHandle, drawPosition, drawPosition + new Vector2(size, size));
                drawList.PopClipRect();
            }

            Portrait.Draw(healthY);

            if (FinalFantasy7Plugin.Ui.Configuration.ClassIconEnabled)
            {
                float iconSize = FinalFantasy7Plugin.Ui.Configuration.ClassIconScale;

                if (FinalFantasy7Plugin.Cs.LocalPlayer is null) return;

                
                ImageDrawing.DrawIcon(drawList, (ushort)(62000 + FinalFantasy7Plugin.Cs.LocalPlayer.ClassJob.RowId),
                    new Vector2(iconSize, iconSize),
                    //new Vector2((int)(size / 2f), (int)(size / 2f + 18 * FinalFantasy7Plugin.Ui.Configuration.Scale)) +
                    new Vector2((int)(FinalFantasy7Plugin.Ui.Configuration.ClassIconX), (int)(FinalFantasy7Plugin.Ui.Configuration.ClassIconY)) +
                    new Vector2(0, (int)(healthY * FinalFantasy7Plugin.Ui.Configuration.ClassIconScale * FinalFantasy7Plugin.Ui.Configuration.Scale)));
            }

            if (FinalFantasy7Plugin.Ui.Configuration.LevelEnabled)
                ImGuiAdditions.TextShadowedDrawList(drawList, FinalFantasy7Plugin.Ui.Configuration.LevelTextSize,
                    $"Lv{FinalFantasy7Plugin.Cs.LocalPlayer?.Level}",
                    drawPosition + new Vector2(FinalFantasy7Plugin.Ui.Configuration.LevelTextX, FinalFantasy7Plugin.Ui.Configuration.LevelTextY) * FinalFantasy7Plugin.Ui.Configuration.Scale,
                    new Vector4(249 / 255f, 247 / 255f, 232 / 255f, 0.9f),
                    new Vector4(96 / 255f, 78 / 255f, 23 / 255f, 0.25f), 3,
                    FinalFantasy7Plugin.Ui.Configuration.LevelTextAlignment);

            if (FinalFantasy7Plugin.Ui.Configuration.ExpValueTextEnabled)
                ImGuiAdditions.TextShadowedDrawList(drawList, FinalFantasy7Plugin.Ui.Configuration.ExpValueTextSize,
                    $"{StringFormatting.FormatDigits(Experience, FinalFantasy7Plugin.Ui.Configuration.ExpValueTextFormatStyle)} / {StringFormatting.FormatDigits(MaxExperience, FinalFantasy7Plugin.Ui.Configuration.ExpValueTextFormatStyle)}",
                    drawPosition + new Vector2(FinalFantasy7Plugin.Ui.Configuration.ExpValueTextPositionX, FinalFantasy7Plugin.Ui.Configuration.ExpValueTextPositionY),
                    new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f),
                    new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f),
                    3,
                    (TextAlignment)FinalFantasy7Plugin.Ui.Configuration.ExpValueTextAlignment);
        }

        public unsafe void Dispose()
        {
            ExperienceRing?.Dispose();
            ExperienceRingRest?.Dispose();
            ExperienceRingGain?.Dispose();
            ExperienceRingBg?.Dispose();

            ExperienceRing = null;
            ExperienceRingRest = null;
            ExperienceRingGain = null;
            ExperienceRingBg = null;
            _addonExp = null;
        }

        private uint Experience { get; set; }
        private uint RestedBonusExperience { get; set; }
        private uint MaxExperience { get; set; }
        private uint LastExp { get; set; }
        private uint LastJob { get; set; }
        private byte LastLevel { get; set; }
        private uint ExpBeforeGain { get; set; }
        private float ExpTemp { get; set; }
        private float ExpGainTime { get; set; }
        private Ring? ExperienceRing { get; set; }
        private Ring? ExperienceRingRest { get; set; }
        private Ring? ExperienceRingGain { get; set; }
        private Ring? ExperienceRingBg { get; set; }
    }
}
