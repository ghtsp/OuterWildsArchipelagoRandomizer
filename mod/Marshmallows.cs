﻿using HarmonyLib;
using System;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class Marshmallows
{
    private static uint _normalMarshmallows = 0;
    private static uint _perfectMarshmallows = 0;
    private static uint _burntMarshmallows = 0;

    public static uint normalMarshmallows
    {
        get => _normalMarshmallows;
        set
        {
            if (value > _normalMarshmallows && playerResources != null)
            {
                playerResources._currentHealth = (float)Math.Min(playerResources._currentHealth + 50, PlayerResources._maxHealth);

                Locator.GetPlayerAudioController().PlayMarshmallowEat();
                var nd = new NotificationData(NotificationTarget.Player, "MARSHMALLOW CONSUMED. 50% HEALTH RESTORED.", 3f, false);
                NotificationManager.SharedInstance.PostNotification(nd, false);
            }
            _normalMarshmallows = value;
        }
    }
    public static uint perfectMarshmallows
    {
        get => _perfectMarshmallows;
        set
        {
            if (value > _perfectMarshmallows && playerResources != null)
            {
                playerResources._currentHealth = PlayerResources._maxHealth;

                Locator.GetPlayerAudioController().PlayMarshmallowEat();
                var nd = new NotificationData(NotificationTarget.Player, "PERFECT MARSHMALLOW CONSUMED. HEALTH FULLY RESTORED.", 3f, false);
                NotificationManager.SharedInstance.PostNotification(nd, false);
            }
            _perfectMarshmallows = value;
        }
    }
    public static uint burntMarshmallows
    {
        get => _burntMarshmallows;
        set
        {
            if (value > _burntMarshmallows && playerResources != null)
            {
                Locator.GetPlayerAudioController().PlayMarshmallowEatBurnt();
                var nd = new NotificationData(NotificationTarget.Player, "BURNT MARSHMALLOW CONSUMED. HEALTH UNCHANGED.", 3f, false);
                NotificationManager.SharedInstance.PostNotification(nd, false);
            }
            _burntMarshmallows = value;
        }
    }

    static PlayerResources playerResources = null;

    [HarmonyPrefix, HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.Awake))]
    public static void PlayerResources_Awake(PlayerResources __instance) => playerResources = __instance;
}
