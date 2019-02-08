using ColossalFramework.UI;
using UnityEngine;
using ICities;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using CimTools.Utilities;

namespace NetPicker
{
    public static class ElektrixUI
    {
        // shorthand for writing each sprite's states over and over
        public static void SetupButtonStateSprites(ref UIButton button, string spriteName, bool noNormal = false)
        {
            button.normalBgSprite = spriteName + (noNormal ? "" : "Normal");
            button.hoveredBgSprite = spriteName + "Hovered";
            button.focusedBgSprite = spriteName + "Focused";
            button.pressedBgSprite = spriteName + "Pressed";
            button.disabledBgSprite = spriteName + "Disabled";
        }
    }

    public class NetPickerLoader : LoadingExtensionBase
    {
        public string m_atlasName = "ElektrixNetPicker";
        public bool m_atlasLoaded;
        ElektrixModsConfiguration config = Configuration<ElektrixModsConfiguration>.Load();
        private void LoadSprites()
        {
            if (SpriteUtilities.GetAtlas(m_atlasName) != null) return;

            m_atlasLoaded = SpriteUtilities.InitialiseAtlas(Path.Combine(NetPickerMod.AsmPath, "SpriteAtlas.png"), m_atlasName);
            if (m_atlasLoaded)
            {
                var spriteSuccess = true;
                spriteSuccess = SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(0, 0), new Vector2(128, 128)), "Elektrix", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(128, 0), new Vector2(128, 128)), "NetTool", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(0, 0), new Vector2(20, 20)), "Blank", m_atlasName)
                             && spriteSuccess;
                if (!spriteSuccess) NetPickerTool.instance.ThrowError("Some sprites haven't been loaded. This is abnormal; you should probably report this to the mod creator.");
            }
            else NetPickerTool.instance.ThrowError("The texture atlas (provides custom icons) has not loaded. All icons have reverted to text prompts.");
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (NetPickerTool.instance == null)
            {
                ToolController toolController = GameObject.FindObjectOfType<ToolController>();
                NetPickerTool.instance = toolController.gameObject.AddComponent<NetPickerTool>();
                NetPickerTool.instance.enabled = false;
            }

            // Load Sprites
            LoadSprites();

            // Initiate UI
            // 1.1 - modular?
            UIPanel modsPanel = (UIPanel)UIView.Find("ElektrixModsPanel");
            int toggleClicks = 0;

            if (modsPanel == null)
            {
                UIComponent TSBar = UIView.Find("TSBar");
                UIPanel elektrixModsBackground = TSBar.AddUIComponent<UIPanel>();
                elektrixModsBackground.name = "ElektrixMB";
                elektrixModsBackground.absolutePosition = new Vector2(config.PanelX, config.PanelY);
                elektrixModsBackground.width = 60f;
                elektrixModsBackground.height = 60f;
                elektrixModsBackground.zOrder = 1;

                UIButton doneButton = elektrixModsBackground.AddUIComponent<UIButton>();
                doneButton.normalBgSprite = "GenericPanel";
                doneButton.width = 100f;
                doneButton.height = 50f;
                doneButton.relativePosition = new Vector2(70f, 5f);
                doneButton.text = "Done";
                doneButton.hoveredTextColor = new Color32(0, 255, 255, 1);
                doneButton.Hide();
                doneButton.zOrder = 99;

                UIDragHandle handle = elektrixModsBackground.AddUIComponent<UIDragHandle>();
                handle.name = "ElektrixDragHandle";
                handle.relativePosition = Vector2.zero;
                handle.width = elektrixModsBackground.width - 5;
                handle.height = elektrixModsBackground.height - 5;
                handle.zOrder = 0;
                handle.target = elektrixModsBackground;
                handle.Start();
                handle.enabled = false;

                elektrixModsBackground.zOrder = 9;
                handle.zOrder = 10;

                elektrixModsBackground.eventDoubleClick += (component, ms) =>
                {
                    handle.zOrder = 13;
                    doneButton.Show();
                    handle.enabled = true;
                };

                doneButton.eventClick += (component, ms) =>
                {
                    doneButton.Hide();
                    handle.zOrder = 10;
                    handle.enabled = false;

                    config.PanelX = (int)elektrixModsBackground.absolutePosition.x;
                    config.PanelY = (int)elektrixModsBackground.absolutePosition.y;
                    Configuration<ElektrixModsConfiguration>.Save();
                };

                if (m_atlasLoaded)
                {
                    elektrixModsBackground.atlas = SpriteUtilities.GetAtlas(m_atlasName);
                    elektrixModsBackground.backgroundSprite = "Blank";
                }
                else
                {
                    elektrixModsBackground.backgroundSprite = "GenericPanelLight";
                }


                elektrixModsBackground.color = new Color32(96, 96, 96, 255);

                UIButton elektrixModsToggle = elektrixModsBackground.AddUIComponent<UIButton>();
                elektrixModsToggle.disabledTextColor = new Color32(128, 128, 128, 255);
                ElektrixUI.SetupButtonStateSprites(ref elektrixModsToggle, "ToolbarIconGroup1");
                elektrixModsToggle.relativePosition = new Vector3(5f, 0f);
                elektrixModsToggle.size = new Vector2(45f, 50f);
                elektrixModsToggle.name = "ElektrixModsButton";
                elektrixModsToggle.zOrder = 11;
                if (m_atlasLoaded)
                {
                    UISprite internalSprite = elektrixModsToggle.AddUIComponent<UISprite>();
                    internalSprite.atlas = SpriteUtilities.GetAtlas(m_atlasName);
                    internalSprite.spriteName = "Elektrix";
                    internalSprite.relativePosition = new Vector3(-3, 0);
                    internalSprite.width = internalSprite.height = 50f;
                }
                else
                {
                    elektrixModsToggle.text = "E";
                }
                elektrixModsToggle.textScale = 1.3f;
                elektrixModsToggle.textVerticalAlignment = UIVerticalAlignment.Middle;
                elektrixModsToggle.textHorizontalAlignment = UIHorizontalAlignment.Center;

                modsPanel = elektrixModsBackground.AddUIComponent<UIPanel>();
                modsPanel.backgroundSprite = "GenericPanelLight";
                modsPanel.color = new Color32(96, 96, 96, 255);
                modsPanel.name = "ElektrixModsPanel";
                modsPanel.width = 0;
                modsPanel.relativePosition = new Vector3(0, -modsPanel.height - 7);
                modsPanel.Hide();

                UILabel panelLabel = modsPanel.AddUIComponent<UILabel>();
                panelLabel.text = "Elektrix's Mods";
                panelLabel.relativePosition = new Vector3(12f, 12f);

                elektrixModsToggle.eventClicked += (component, click) =>
                {
                    toggleClicks++;
                    if (toggleClicks == 1)
                    {
                        elektrixModsToggle.Focus();
                        modsPanel.Show();
                    }
                    else
                    {
                        elektrixModsToggle.Unfocus();
                        toggleClicks = 0;
                        modsPanel.Hide();
                    }
                };
            }

            modsPanel = (UIPanel) UIView.Find("ElektrixModsPanel");

            UIPanel backgroundPanel = modsPanel.AddUIComponent<UIPanel>();
            backgroundPanel.backgroundSprite = "GenericPanelLight";
            backgroundPanel.name = "E2";
            backgroundPanel.height = 50f;
            backgroundPanel.width = 135f;
            backgroundPanel.relativePosition = new Vector3(10f, 40f + 70f);

            UIButton netPickerTool = backgroundPanel.AddUIComponent<UIButton>();
            int netClicks = 0;
            ElektrixUI.SetupButtonStateSprites(ref netPickerTool, "OptionBase", true);
            netPickerTool.size = new Vector2(45f, 45f);
            netPickerTool.relativePosition = new Vector3(5f, 2.5f);
            netPickerTool.name = "E2A";
            if (m_atlasLoaded)
            {
                UISprite internalSprite = netPickerTool.AddUIComponent<UISprite>();
                internalSprite.atlas = SpriteUtilities.GetAtlas(m_atlasName);
                internalSprite.spriteName = "NetTool";
                internalSprite.relativePosition = Vector3.zero;
                internalSprite.width = internalSprite.height = 50f;
            }
            else
            {
                netPickerTool.text = "P";
            }
            netPickerTool.textScale = 1.3f;

            // Final overrides
            //UIPanel modsPanel = (UIPanel) UIView.Find("ElektrixModsPanel");
            IList<UIComponent> panels = modsPanel.components;
            float longestPanelWidth = 0;
            for (int i = 0; i < panels.Count; i++){
                if (!(panels[i] is UIPanel)) continue;
                panels[i].relativePosition = new Vector3(panels[i].relativePosition.x, 35 + (60 * (i - 1)));
                if(panels[i].width > longestPanelWidth) {
                    longestPanelWidth = panels[i].width;
                }
            }

            modsPanel.height = 40f + (modsPanel.childCount - 1) * 60f;
            modsPanel.width = 20f + longestPanelWidth;
            modsPanel.relativePosition = new Vector3(0, -modsPanel.height - 7);

            // Events
            netPickerTool.eventClicked += (component, click) =>
            {
                if(!NetPickerTool.instance.enabled){
                    netClicks = 0;
                }

                netClicks++;
                if (netClicks == 1)
                {
                    netPickerTool.Focus();
                    NetPickerTool.instance.enabled = true;
                }
                else
                {
                    netPickerTool.Unfocus();
                    netClicks = 0;
                    NetPickerTool.instance.enabled = false;
                    ToolsModifierControl.SetTool<DefaultTool>();
                }
            };
        }
    }
}
