﻿using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using FinalFantasy7Plugin.Configuration;
using FinalFantasy7Plugin.UIElements.Experience;
using Lumina.Excel.Sheets;
using Dalamud.Plugin.Services;

namespace FinalFantasy7Plugin
{
    public sealed class FinalFantasy7Plugin : IDalamudPlugin
    {
        public string Name => "Final Fantasy 7 Remake/Rebirth UI Plugin";

        private const string SettingsCommand = "/ff7rconfig";
        private const string ToggleCommand = "/ff7r";

        public static string TemplateLocation = "";

        public FinalFantasy7Plugin(
            IDalamudPluginInterface pluginInterface,
            IFramework framework,
            ICommandManager commandManager,
            IClientState clientState,
            IGameGui gameGui,
            IDataManager dataManager,
            ITextureProvider textureProvider,
            IPluginLog pluginLog

            )
        {
            Pi = pluginInterface;
            Fw = framework;
            Cm = commandManager;
            Cs = clientState;
            Gui = gameGui;
            Dm = dataManager;
            Tp = textureProvider;
            Pl = pluginLog;


            Timer = Stopwatch.StartNew();

            TemplateLocation = Path.GetDirectoryName(pluginInterface.AssemblyLocation.FullName!) ?? "";

            var configuration = Pi.GetPluginConfig() as Settings ?? new Settings();
            configuration.Initialize(Pi);

            Ui = new PluginUI(configuration);

            Portrait.SetAllPortraits();

            Fw.Update += OnUpdate;


            Cm.AddHandler(SettingsCommand, new CommandInfo(OnSettingsCommand)
            {
                HelpMessage = "Opens configuration for Final Fantasy 7 Remake/Rebirth UI Bars."
            });

            Cm.AddHandler(ToggleCommand, new CommandInfo(OnToggleCommand)
            {
                HelpMessage = "Toggles the FF7:R UI."
            });

            Pi.UiBuilder.Draw += DrawUi;
            Pi.UiBuilder.OpenMainUi += ToggleMainVisibility;
            Pi.UiBuilder.OpenConfigUi += DrawConfigUi;
            Cs.TerritoryChanged += OnTerritoryChange;

            if (Cs.LocalPlayer != null)
            {
                IsInPvp = GetTerritoryPvP(Cs.TerritoryType);
            }
        }

        public void Dispose()
        {
            Ui?.Dispose();

            Cm.RemoveHandler(SettingsCommand);
            Cm.RemoveHandler(ToggleCommand);

            Fw.Update -= OnUpdate;

            Pi.UiBuilder.Draw -= DrawUi;
            Pi.UiBuilder.OpenMainUi -= ToggleMainVisibility;
            Pi.UiBuilder.OpenConfigUi -= DrawConfigUi;
            Cs.TerritoryChanged -= OnTerritoryChange;

            Timer = null;
        }

        private void OnUpdate(IFramework framework)
        {
            if (Timer is null) return;

            UiSpeed = Timer.ElapsedMilliseconds / 1000f;
            Timer.Restart();
            Ui.OnUpdate();
        }

        private void ToggleMainVisibility()
        {
            Ui.Configuration.Enabled = !Ui.Configuration.Enabled;
        }

        private void OnSettingsCommand(string command, string args)
        {
            DrawConfigUi();
        }
        private void OnToggleCommand(string command, string args)
        {
            Ui.Configuration.Enabled = !Ui.Configuration.Enabled;
            Ui.Configuration.Save();
        }

        private void OnTerritoryChange(ushort e)
        {
            IsInPvp = GetTerritoryPvP(e);
        }

        private void DrawUi()
        {
            Ui.Draw();
        }

        private void DrawConfigUi()
        {
            Ui.SettingsVisible = true;
        }

        private bool GetTerritoryPvP(uint territoryType)
        {
            try
            {
                var territory = Dm.GetExcelSheet<TerritoryType>().GetRow(territoryType);
                return territory.IsPvpZone;
            }
            catch (KeyNotFoundException)
            {
                Pl.Warning("Could not get territory for current zone");
                return false;
            }
        }
        public static CultureInfo GetCulture()
        {
            try
            {
                return CultureInfo.GetCultureInfo(Ui.Configuration.TextFormatCulture);
            }
            catch
            {
                return CultureInfo.GetCultureInfo("en-US");
            }
        }

        public static IDalamudPluginInterface Pi { get; private set; } = null!;
        public static IFramework Fw { get; private set; } = null!;
        public static ICommandManager Cm { get; private set; } = null!;
        public static IClientState Cs { get; private set; } = null!;
        public static IGameGui Gui { get; private set; } = null!;
        public static IDataManager Dm { get; private set; } = null!;
        public static IPluginLog Pl { get; private set; } = null!;
        public static ITextureProvider Tp { get; private set; } = null!;

        public static PluginUI Ui { get; private set; } = null!;

        public static Stopwatch? Timer { get; private set; }
        public static float UiSpeed { get; set; }
        public static bool IsInPvp { get; private set; }

    }
}
