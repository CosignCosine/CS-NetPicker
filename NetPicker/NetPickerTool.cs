using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace NetPicker
{
    public class Db {
        public static bool on = false;

        public static void l (object m) {
            if(on) Debug.Log(m);
        }

        public static void w (object m) {
            if (on) Debug.LogWarning(m);
        }

        public static void e(object m)
        {
            if (on) Debug.LogWarning(m);
        }
    }

    public class NetPickerTool : ToolBase
    {
        public static NetPickerTool instance;

        // road cache
        public List<string>            NETPICKER_ROADCACHE_STRINGS = new List<string>();
        public List<List<UIComponent>> NETPICKER_ROADCACHE_DICTIONARY = new List<List<UIComponent>>();

        ushort m_hover;

        public NetInfo m_netInfo;
        public NetTool m_netTool;
        public bool m_fakeNetTool;

        Color hcolor = new Color32(0, 181, 255, 255);
        Color scolor = new Color32(95, 166, 0, 244);

        // Network Skins compatibility
        /*
        public PluginManager.PluginInfo NetworkSkins {
            get {
                return PluginManager.instance.GetPluginsInfo()
                    .Where(mod => (
                        mod.publishedFileID.AsUInt64 == 543722850uL || 
                        mod.name.Contains("Network Skins") || 
                        mod.name.Contains("NetworkSkins")) &&
                        mod.isEnabled
                    )
                    .FirstOrDefault();
            }
        }*/
        public Assembly m_networkSkinsAssembly;

        public NetManager Manager
        {
            get { return Singleton<NetManager>.instance; }
        }
        public NetNode GetNode(ushort id)
        {
            return Manager.m_nodes.m_buffer[id];
        }
        public NetSegment GetSegment(ushort id)
        {
            return Manager.m_segments.m_buffer[id];
        }
        public void ThrowError(string message)
        {
            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage("Net Picker", message, false);
        }
        public NetInfo FindDefaultElevation(NetInfo info){
            for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); i++)
            {
                NetInfo prefab = PrefabCollection<NetInfo>.GetLoaded(i);
                if((AssetEditorRoadUtils.TryGetBridge(prefab) != null && AssetEditorRoadUtils.TryGetBridge(prefab).name == info.name) || 
                   (AssetEditorRoadUtils.TryGetElevated(prefab) != null && AssetEditorRoadUtils.TryGetElevated(prefab).name == info.name) || 
                   (AssetEditorRoadUtils.TryGetSlope(prefab) != null && AssetEditorRoadUtils.TryGetSlope(prefab).name == info.name) || 
                   (AssetEditorRoadUtils.TryGetTunnel(prefab) != null && AssetEditorRoadUtils.TryGetTunnel(prefab).name == info.name) ){
                    return prefab;
                }
            }
            return info;
        }

        public List<UIComponent> FindRoadInPanel(string name){
            return FindRoadInPanel(name, 0);
        }

        public List<UIComponent> FindRoadInPanel(string name, int attemptNumber){
            if (NETPICKER_ROADCACHE_STRINGS.Contains(name)) return NETPICKER_ROADCACHE_DICTIONARY[NETPICKER_ROADCACHE_STRINGS.IndexOf(name)];

            List<UIComponent> result = new List<UIComponent>();
            string[] panels = { "RoadsPanel", "PublicTransportPanel", "BeautificationPanel", "LandscapingPanel", "ElectricityPanel" };

            // If this isn't the first attempt at finding the network (in RoadsPanel) then 
            if (attemptNumber > 0) UIView.Find(panels[attemptNumber - 1]).Hide();

            UIView.Find(panels[attemptNumber]).Show();
            Db.l(panels[attemptNumber]);
            List<UIButton> hide = UIView.Find(panels[attemptNumber]).GetComponentsInChildren<UITabstrip>()[0].GetComponentsInChildren<UIButton>().ToList();

            for (var i = 0; i < hide.Count; i++){
                hide[i].SimulateClick();

                UIPanel testedPanel = null;
                UIComponent GTSContainer = UIView.Find(panels[attemptNumber]).GetComponentsInChildren<UITabContainer>()[0];
                for (var k = 0; k < GTSContainer.GetComponentsInChildren<UIPanel>().ToList().Count; k++){
                    UIPanel t = GTSContainer.GetComponentsInChildren<UIPanel>()[k];
                    if(t.isVisible) {
                        testedPanel = t;
                        break;
                    }
                }
                if (testedPanel == null) return null;

                for (var j = 0; j < testedPanel.GetComponentsInChildren<UIButton>().ToList().Count; j++)
                {
                    UIButton button = testedPanel.GetComponentsInChildren<UIButton>().ToList()[j];
                    Db.w("[Net Picker] Looking for " + name + " ?= " + button.name + " [" + testedPanel.name + "]");
                    if(!NETPICKER_ROADCACHE_STRINGS.Contains(button.name)){
                        List<UIComponent> cacheBuilder = new List<UIComponent>();
                        cacheBuilder.Add(hide[i]);
                        cacheBuilder.Add(button);
                        NETPICKER_ROADCACHE_STRINGS.Add(button.name);
                        NETPICKER_ROADCACHE_DICTIONARY.Add(cacheBuilder);
                    }
                    if (button.name == name)
                    {
                        result.Add(hide[i]);
                        result.Add(button);
                        UIView.Find(panels[attemptNumber]).Hide();
                        return result;
                    }
                }
            }
            attemptNumber++;
            if(attemptNumber < 5){
                return FindRoadInPanel(name, attemptNumber);
            }else{
                return null;
            }
        }
        protected override void OnToolUpdate()
        {
            base.OnToolUpdate();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastInput input = new RaycastInput(ray, Camera.main.farClipPlane);
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            input.m_ignoreNodeFlags = NetNode.Flags.All;
            input.m_ignoreParkFlags = DistrictPark.Flags.All;
            input.m_ignorePropFlags = PropInstance.Flags.All;
            input.m_ignoreTreeFlags = TreeInstance.Flags.All;
            input.m_ignoreCitizenFlags = CitizenInstance.Flags.All;
            input.m_ignoreVehicleFlags = Vehicle.Flags.Created;
            input.m_ignoreBuildingFlags = Building.Flags.All;
            input.m_ignoreDisasterFlags = DisasterData.Flags.All;
            input.m_ignoreTransportFlags = TransportLine.Flags.All;
            input.m_ignoreParkedVehicleFlags = VehicleParked.Flags.All;
            input.m_ignoreTerrain = true;
            RayCast(input, out RaycastOutput output);
            m_hover = output.m_netSegment;

            if(Input.GetKeyDown(KeyCode.Escape)){
                enabled = false;
                ToolsModifierControl.SetTool<DefaultTool>();
            }

            if(m_hover != 0) {
                m_netInfo = GetSegment(m_hover).Info;
                if(Input.GetMouseButtonUp(0)){
                    instance.enabled = false;
                    if(Singleton<UnlockManager>.instance.Unlocked(m_netInfo.m_UnlockMilestone)){
                        
                        // you know when you make a bugfix just to mess with people? well this is that bugfix. enjoy.
                        UIView.Find("E2A").Unfocus();

                        UIView.Find("TSCloseButton").SimulateClick();

                        enabled = false;

                        m_netTool = ToolsModifierControl.SetTool<NetTool>();

                        m_netInfo = FindDefaultElevation(m_netInfo);

                        // If we don't load UI, stuff happens, whatever.
                        List<UIComponent> reveal = null;
                        ElektrixModsConfiguration config = Configuration<ElektrixModsConfiguration>.Load();
                        if (config.NP_OpenUI) reveal = FindRoadInPanel(m_netInfo.name);

                        m_netTool.Prefab = m_netInfo;
                        if(reveal != null){
                            UIView.Find("TSCloseButton").SimulateClick();
                            Db.l("[Net Picker] Attempting to open panel " + reveal[1].parent.parent.parent.parent.name.Replace("Panel", ""));
                            UIButton rb = UIView.Find("MainToolstrip").Find<UIButton>(reveal[1].parent.parent.parent.parent.name.Replace("Panel", ""));
                            rb.SimulateClick();
                            reveal[0].SimulateClick();
                            reveal[1].SimulateClick();
                            if (!UIView.Find("TSCloseButton").isVisible) Db.l("Failed");
                        } else if (config.NP_OpenUI) {
                            ThrowError("This net type is hidden and won't work properly if used by non-advanced users. In order to use this net, disable 'open ui' in Net Picker settings. If this net *isn't* actually hidden, please tweet your net type (and what menu it can be found in) to @cosigncosine. Thanks!");
                            ToolsModifierControl.SetTool<DefaultTool>();
                            UIView.Find("ElectricityPanel").Hide();
                        }
                        m_fakeNetTool = true;

                        //Debug.LogError(NetworkSkins.modPath);
                        ushort segmentId = m_hover;
                        NetInfo prefab = m_netInfo;

                        try
                        {
                            Type segmentDataManagerType = Type.GetType("NetworkSkins.Data.SegmentDataManager, NetworkSkins");
                            object segmentDataManager = segmentDataManagerType.GetField("Instance").GetValue(null);
                            object[] SegmentToSegmentDataMap = (object[])segmentDataManagerType.GetField("SegmentToSegmentDataMap").GetValue(segmentDataManager);

                            var segmentData = SegmentToSegmentDataMap[segmentId];
                            segmentDataManagerType.GetMethod("SetActiveOptions").Invoke(segmentDataManager, new object[] { prefab, segmentData });
                        }
                        catch (Exception e) { Debug.Log("Network skins isn't installed.");  }

                        if (config.CloseWindow) UIView.Find("ElektrixModsPanel").Hide();
                    }else{
                        ThrowError("This net type isn't unlocked yet! Wait until this unlock/milestone: " + m_netInfo.m_UnlockMilestone.m_name);
                    }
                }
            }else{
                m_netInfo = default(NetInfo);
            }
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);
            if (enabled == true)
            {
                if (m_hover != 0)
                {
                    NetSegment hoveredSegment = GetSegment(m_hover);
                    NetTool.RenderOverlay(cameraInfo, ref hoveredSegment, hcolor, new Color(1f, 0f, 0f, 1f));
                }
            }
        }
    }
}
