using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Framework.Water;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace BarricadeMisc
{
    public class BarricadeMisc : RocketPlugin<BarricadeMiscConfiguration>
    {
        public BarricadeMisc Instance { get; private set; }
        public BarricadeMiscConfiguration Config { get; private set; }
        public Color MessageColour { get; private set; }

        protected override void Load()
        {
            Instance = this;
            Config = Configuration.Instance;

            MessageColour = UnturnedChat.GetColorFromName(Config.MessageColour, Color.green);

            BarricadeManager.onDeployBarricadeRequested += OnDeployBarricadeRequested;

            Logger.Log($"{Name} {Assembly.GetName().Version} by Gamingtoday093 has been Loaded");
        }

        protected override void Unload()
        {
            BarricadeManager.onDeployBarricadeRequested -= OnDeployBarricadeRequested;

            Logger.Log($"{Name} has been Unloaded");
        }

        private void OnDeployBarricadeRequested(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(owner));
            if (player?.Player == null) return;
            if (player.HasPermission("BarricadeMisc.Bypass")) return;

            if (Config.BlockUnderWaterBeds && asset.build == EBuild.BED && WaterUtility.isPointUnderwater(point))
            {
                UnturnedChat.Say(player, Translate("BlockBeds"), MessageColour, true);

                shouldAllow = false;
                return;
            }

            if (Config.BlockVehicleBuilding && hit != null && hit.CompareTag("Vehicle"))
            {
                InteractableVehicle vehicle = DamageTool.getVehicle(hit.root);

                if (Config.IgnoreVehicles.Contains(vehicle.asset.id)) return;

                if ((!Config.Builds.Contains(asset.build) != Config.IsBuildsBlacklist) && (!Config.Barricades.Contains(asset.id) != Config.IsBarricadesBlacklist))
                {
                    UnturnedChat.Say(player, Translate("BlockBuild"), MessageColour, true);

                    shouldAllow = false;
                    return;
                }

                if (Config.UseMaximumBarricadesAllowedIgnoreBuild)
                {
                    BarricadeRegion vehicleBarricadeRegion = BarricadeManager.getRegionFromVehicle(vehicle);
                    if (vehicleBarricadeRegion != null && Config.Barricades.Contains(asset.id) != Config.IsBarricadesBlacklist &&
                        vehicleBarricadeRegion.drops.Where(b => Config.Barricades.Contains(b.asset.id) != Config.IsBarricadesBlacklist).Count() >= Config.MaximumBarricadesAllowedIgnoreBuild)
                    {
                        UnturnedChat.Say(player, Translate("BlockMaximum"), MessageColour, true);

                        shouldAllow = false;
                        return;
                    }
                }

                if (Config.BlockBarricadesOnSeats)
                {
                    GameObject barricadeGameObject = Instantiate(asset.barricade, hit);
                    barricadeGameObject.name = "BarricadeMisc/SeatCheck";
                    barricadeGameObject.transform.localPosition = point;
                    barricadeGameObject.transform.localRotation = BarricadeManager.getRotation(asset, angle_x, angle_y, angle_z);
                    barricadeGameObject.transform.localScale = Vector3.one;
                    Physics.SyncTransforms();
                    for (int p = 0; p < vehicle.passengers.Length; p++)
                    {
                        Transform seat = vehicle.passengers[p].seat;
                        Vector3 eyePosition = seat.position + (seat.up * 1.5f);
                        bool forward = Physics.Raycast(eyePosition, seat.forward, out RaycastHit forwardHit, Config.BlockBarricadesOnSeatsRaycastDistance);
                        bool right = Physics.Raycast(eyePosition, seat.right, out RaycastHit rightHit, Config.BlockBarricadesOnSeatsRaycastDistance);
                        bool left = Physics.Raycast(eyePosition, -seat.right, out RaycastHit leftHit, Config.BlockBarricadesOnSeatsRaycastDistance);

                        if ((forward && IsChildOf(forwardHit.collider.transform, "BarricadeMisc/SeatCheck")) ||
                            (right && IsChildOf(rightHit.collider.transform, "BarricadeMisc/SeatCheck")) ||
                            (left && IsChildOf(leftHit.collider.transform, "BarricadeMisc/SeatCheck")))
                        {
                            shouldAllow = false;
                            break;
                        }
                    }
                    Destroy(barricadeGameObject);

                    if (!shouldAllow)
                        UnturnedChat.Say(player, Translate("BlockSeat"), MessageColour, true);
                }
            }
        }

        private bool IsChildOf(Transform transform, string name)
        {
            if (transform.name == name) return true;
            if (transform.parent == null) return false;
            return IsChildOf(transform.parent, name);
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "BlockBeds", "You are not Allowed to place Beds Underwater!" },
            { "BlockBuild", "You are not Allowed to place this Barricade on this Vehicle!" },
            { "BlockMaximum", "You are not Allowed to place anymore Barricades of this Type on this Vehicle!" },
            { "BlockSeat", "You are not Allowed to block Vehicle Seats!" }
        };
    }
}
