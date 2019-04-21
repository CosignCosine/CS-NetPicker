using System;
namespace NetPicker
{

    [ConfigurationPath("ElektrixModsConfig.xml")]
    public class ElektrixModsConfiguration
    {
        public int PanelX { get; set; } = 500;
        public int PanelY { get; set; } = 500;

        public bool HasSeenHowToDragTheStupidIcon { get; set; } = false;

        public bool CloseWindow { get; set; } = false;

        public bool RO_DisableWarnings { get; set; } = false;
        public bool NP_OpenUI { get; set; } = true;
        public float NP_SegmentSplitPrecision { get; set; } = 60f;

    }
}
