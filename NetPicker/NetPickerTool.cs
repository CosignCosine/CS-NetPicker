using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using UnityEngine;
using System;
using System.Reflection;

namespace NetPicker
{
    public class NetPickerTool : ToolBase
    {
        public static NetPickerTool instance;

        ushort m_hover;

        public NetInfo m_netInfo;
        public NetTool m_netTool;
        public bool m_fakeNetTool;

        // Network Skins compatibility
        /*
        public bool m_networkSkinsInstallation {
            get {
                var pluginManager = PluginManager.instance;
                var plugins = pluginManager.GetPluginsInfo();

                foreach (var item in plugins)
                {
                    try
                    {
                        if(item.name == "543722850" || item.name == "NetworkSkins"){
                            if(m_networkSkinsAssembly == null){
                                m_networkSkinsAssembly = Assembly.LoadFrom(item.modPath);
                            }
                            //return true;
                            return false; // @TODO implement network skins compatibility. disabled because need a legitimate update
                        }
                    }
                    catch
                    {
                        
                    }
                }
                return false;
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

            if(m_hover != 0) {
                m_netInfo = GetSegment(m_hover).Info;
                if(Input.GetMouseButtonUp(0)){
                    instance.enabled = false;
                    if(Singleton<UnlockManager>.instance.Unlocked(m_netInfo.m_UnlockMilestone)){
                        
                        // you know when you make a bugfix just to mess with people? well this is that bugfix. enjoy.
                        UIView.Find("E2A").Unfocus();

                        enabled = false;

                        m_netTool = ToolsModifierControl.SetTool<NetTool>();

                        m_netInfo = FindDefaultElevation(m_netInfo);


                        // Network skins compatibility
                        //if (m_networkSkinsInstallation)
                        //{
                            //ushort segmentId = m_hover;
                            //NetInfo prefab = m_netInfo;

                            //Type SegmentDataManager = m_networkSkinsAssembly.GetType("NetworkSkins.Data.SegmentDataManager");

                            //object networkSkins = Activator.CreateInstance(SegmentDataManager);

                            //var segmentData = SegmentDataManager.GetField("SegmentToSegmentDataMap").GetValue(networkSkins)[segmentId];

                            //MethodInfo m = SegmentDataManager.GetMethod("SetActiveOptions");
                            //m.Invoke(networkSkins, new object[] { prefab, segmentData });
                        //}

                        m_netTool.Prefab = m_netInfo;
                        m_fakeNetTool = true;
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
                    NetTool.RenderOverlay(cameraInfo, ref hoveredSegment, new Color(0f, 0f, 1f, 1f), new Color(1f, 0f, 0f, 1f));
                }
            }
        }
    }
}
