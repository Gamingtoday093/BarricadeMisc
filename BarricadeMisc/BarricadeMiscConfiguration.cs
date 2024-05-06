using Rocket.API;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BarricadeMisc
{
    public class BarricadeMiscConfiguration : IRocketPluginConfiguration
    {
        public string MessageColour { get; set; }

        public bool BlockUnderWaterBeds { get; set; }

        public bool BlockVehicleBuilding { get; set; }
        public bool BlockBarricadesOnSeats { get; set; }
        public float BlockBarricadesOnSeatsRaycastDistance { get; set; }
        public bool IsBuildsBlacklist { get; set; }
        public EBuild[] Builds { get; set; }
        public bool IsBarricadesBlacklist { get; set; }
        [XmlArrayItem("BarricadeID")]
        public ushort[] Barricades { get; set; }
        public bool UseMaximumBarricadesAllowedIgnoreBuild { get; set; }
        public int MaximumBarricadesAllowedIgnoreBuild { get; set; }
        [XmlArrayItem("VehicleID")]
        public ushort[] IgnoreVehicles { get; set; }

        public void LoadDefaults()
        {
            MessageColour = "yellow";

            BlockUnderWaterBeds = true;

            BlockVehicleBuilding = true;
            BlockBarricadesOnSeats = true;
            BlockBarricadesOnSeatsRaycastDistance = 4f;
            IsBuildsBlacklist = false;
            Builds = new EBuild[]
            {
                EBuild.STORAGE,
                EBuild.STORAGE_WALL,
                EBuild.STEREO,
                EBuild.CHARGE
            };
            IsBarricadesBlacklist = false;
            Barricades = new ushort[]
            {
                1467,
                1468,
                1469,
                1470
            };
            UseMaximumBarricadesAllowedIgnoreBuild = true;
            MaximumBarricadesAllowedIgnoreBuild = 2;
            IgnoreVehicles = new ushort[]
            {
                21002,
                19003
            };
        }
    }
}
