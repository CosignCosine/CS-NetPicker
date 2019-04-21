using UnityEngine;
using ICities;
using ColossalFramework.UI;

namespace NetPicker
{
    public class NetToolFix : ThreadingExtensionBase
    {
        // When one presses "ESC" they expect tools to be cleared. This fix returns that functionality.
        public override void OnAfterSimulationFrame()
        {
            base.OnAfterSimulationFrame();
        
            if(Input.GetKey(KeyCode.Escape) && UIView.Find("ElektrixModsPanel") != null && UIView.Find("ElektrixModsPanel").isVisible){
                UIView.Find("ElektrixModsButton").SimulateClick();
                return;
            }
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return)) && NetPickerTool.instance != null && NetPickerTool.instance.m_fakeNetTool)
            {
                ToolsModifierControl.GetTool<NetTool>().enabled = false;
                NetPickerTool.instance.m_fakeNetTool = false;
                ToolsModifierControl.SetTool<DefaultTool>();
            }

            // The strangest bugfix known to man
            if(ToolsModifierControl.toolController.CurrentTool == null){
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }
    }
}
