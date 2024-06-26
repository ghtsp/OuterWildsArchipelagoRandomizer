﻿using HarmonyLib;

namespace ArchipelagoRandomizer
{
    [HarmonyPatch]
    internal class NomaiTextQoL
    {
        public static bool AutoNomaiText;
        public static float TranslateTime = 0.2f;

        // Auto-expand all Nomai text in the game as a Quality of Life feature
        [HarmonyPostfix, HarmonyPatch(typeof(NomaiWallText), nameof(NomaiWallText.LateInitialize))]
        public static void NomaiWallText_LateInitialize_Postfix(NomaiWallText __instance)
        {
            if (!AutoNomaiText) return;
            foreach (NomaiTextLine child in __instance.GetComponentsInChildren<NomaiTextLine>())
            {
                child._state = NomaiTextLine.VisualState.UNREAD;
            }

            // Ignore scrolls if they aren't socketed
            bool isScroll = __instance.transform.GetComponentInParent<ScrollItem>() != null;
            bool isSocketed = __instance.transform.GetComponentInParent<ScrollSocket>() != null;
            bool isAProjectionWall = __instance.transform.GetComponentInParent<NomaiSharedWhiteboard>() != null;

            if ((!isScroll || isSocketed) && !isAProjectionWall)
            {
                __instance.ShowImmediate();
            }
        }

        // fixes for the text not becoming properly grey when read
        [HarmonyReversePatch, HarmonyPatch(typeof(NomaiText), nameof(NomaiText.SetAsTranslated))]
        public static void base_SetAsTranslated(NomaiText instance, int id) { }

        [HarmonyPrefix, HarmonyPatch(typeof(NomaiWallText), nameof(NomaiWallText.SetAsTranslated))]
        public static bool NomaiWallText_SetAsTranslated_Prefix(NomaiWallText __instance, ref int id)
        {
            if (!AutoNomaiText) return true;
            // This code is copied from the base game and overrides the original method
            base_SetAsTranslated(__instance, id);
            bool revealedChildren = false;
            if (__instance._idToNodeDict.ContainsKey(id))
            {
                if (__instance._useLegacyRevealAnimation) // legacy code, is probably never used but keeping it just in case
                {
                    if (__instance._idToNodeDict.ContainsKey(id))
                    {
                        bool flag2 = false;
                        if (!__instance._idToNodeDict[id].Value.IsTranslated())
                        {
                            if (id == 1)
                            {
                                __instance._idToNodeDict[id].Value.SetTranslatedState(false);
                                flag2 = true;
                            }
                            else if (__instance.IsTranslated(id - 1))
                            {
                                __instance._idToNodeDict[id].Value.SetTranslatedState(false);
                                flag2 = true;
                            }
                        }
                        if (flag2 && __instance._idToNodeDict.ContainsKey(id + 1))
                        {
                            __instance._idToNodeDict[id + 1].Value.SetUnreadState(false);
                            __instance._idToNodeDict[id + 1].Value.SetActive(true, true);
                        }
                    }
                }
                else
                {
                    NomaiTextLine value = __instance._idToNodeDict[id].Value;
                    if (__instance._lineToNodeDict.ContainsKey(value))
                    {
                        OWTreeNode<NomaiTextLine> owtreeNode = __instance._lineToNodeDict[value];
                        if (!__instance._idToNodeDict[id].Value.IsTranslated())
                        {
                            __instance._idToNodeDict[id].Value.SetTranslatedState(false);
                            // The following code is normally responsible for expanding the next arc and sets its color (despite its color having already been set)
                            // Since we don't need it to do any of that, we cut out most of the code and checks it performs
                            // This is left in just in case "revealedChildren" is used in other parts of code or by other mods
                            if (!NomaiWallText.s_revealAllAtStart)
                            {
                                for (int i = 0; i < owtreeNode.Children.Count; i++)
                                {
                                    revealedChildren = true;
                                }
                            }
                        }
                    }
                }
                if (revealedChildren)
                {
                    Locator.GetPlayerAudioController().PlayNomaiTextReveal(__instance);
                }
                
            }
            return false;
        }

        // instant Nomai text
        [HarmonyPrefix, HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.SwitchTextNode))]
        public static void NomaiTranslatorProp_SwitchTextNode_Prefix(NomaiTranslatorProp __instance)
        {
            __instance._totalTranslateTime = TranslateTime;
        }
    }
}
