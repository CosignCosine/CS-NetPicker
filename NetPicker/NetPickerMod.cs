using ICities;
using ColossalFramework.Plugins;
using System;
using System.Linq;
using UnityEngine;

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
    }
}
