using ICities;
using ColossalFramework.Plugins;
using System;
using System.Linq;
using UnityEngine;
using ColossalFramework.UI;

namespace NetPicker
{
    public class NetPickerMod : IUserMod
    {
        public string Name => "Net Picker";
        public string Description => "Elektrix • A mod that allows users to pick nets from ingame instead of from the menu. " + DebugInfo;
        public static string DebugInfo = "";

        // Once again this code is taken directly from Network Skins and is adjusted for context. 
        // https://github.com/boformer/NetworkSkins/blob/master/NetworkSkins/NetworkSkinsMod.cs
        public static string AsmPath => PluginInfo.modPath;
        private static PluginManager.PluginInfo PluginInfo
        {
            get
            {
                var pluginManager = PluginManager.instance;
                var plugins = pluginManager.GetPluginsInfo();

                foreach (var item in plugins)
                {
                    try
                    {
                        var instances = item.GetInstances<IUserMod>();
                        if (!(instances.FirstOrDefault() is NetPickerMod))
                        {
                            continue;
                        }
                        return item;
                    }
                    catch
                    {

                    }
                }
                throw new Exception("Could not find assembly");

            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            // Load the configuration
            ElektrixModsConfiguration config = Configuration<ElektrixModsConfiguration>.Load();
            UIHelperBase globalSettings = helper.AddGroup("Global Settings");
            globalSettings.AddButton("Reset Button Location to (500, 500)", () =>
            {
                config.PanelX = 500;
                config.PanelY = 500;
                if (NetPickerTool.instance != null) UIView.Find("ElektrixMB").absolutePosition = new Vector2(config.PanelX, config.PanelY);
                Configuration<ElektrixModsConfiguration>.Save();
            });
            globalSettings.AddSpace(10);
            globalSettings.AddSlider("Panel X", 0, Screen.currentResolution.width, 1.0f, config.PanelX, (nv) =>
            {
                config.PanelX = (int)nv;
                if (NetPickerTool.instance != null) UIView.Find("ElektrixMB").absolutePosition = new Vector2(config.PanelX, config.PanelY);
                Configuration<ElektrixModsConfiguration>.Save();
            });
            globalSettings.AddSlider("Panel Y", 0, Screen.currentResolution.height, 1.0f, config.PanelY, (nv) =>
            {
                config.PanelY = (int)nv;
                if (NetPickerTool.instance != null) UIView.Find("ElektrixMB").absolutePosition = new Vector2(config.PanelX, config.PanelY);
                Configuration<ElektrixModsConfiguration>.Save();
            });
            globalSettings.AddSpace(10);
            globalSettings.AddCheckbox("Close Window after tool finishes", config.CloseWindow, (value) =>
            {
                config.CloseWindow = value;
                Configuration<ElektrixModsConfiguration>.Save();
            });

            UIHelperBase internalSettings = helper.AddGroup("Mod Settings");
            internalSettings.AddCheckbox("Open UI to Road Location (additionally disables hidden roads)", config.NP_OpenUI, (check) =>
            {
                config.NP_OpenUI = check;
                Configuration<ElektrixModsConfiguration>.Save();
            });
        }
    }
}
