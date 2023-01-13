using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using KBEngine;
using BepInEx.Logging;

namespace QinglongTalentPatch
{
    [BepInPlugin("me.jacklee.mcs.patch.qinglong", "奶油青龙血脉天赋补丁", "1.0.0")]
    public class QinglongTalentPatch : BaseUnityPlugin
    {
        const int QinnglongTianfutId = 31313;
        const int ZhenlongTianfutId = 31336;
        const int OriginalTianFuId = 312;

        static ManualLogSource logger;

        void Start()
        {
            logger = this.Logger;
            logger.LogInfo("JackMod : 奶油龙族血脉天赋补丁 已加载");
            Harmony.CreateAndPatchAll(typeof(QinglongTalentPatch));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerEx), "HasTianFu")]
        public static bool PlayerEx_HasTianFu_Prefix(int tianFuID, ref bool __result)
        {
            if (tianFuID == OriginalTianFuId)
            {
                logger.LogInfo("JackMod : 何人在鉴定本座血脉？");
                if (Tools.instance.CheckHasTianFu(QinnglongTianfutId) || Tools.instance.CheckHasTianFu(ZhenlongTianfutId))
                {
                    logger.LogInfo("JackMod : 龙族补丁真，鉴定为真龙！ (Patch成功)");
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }
}
