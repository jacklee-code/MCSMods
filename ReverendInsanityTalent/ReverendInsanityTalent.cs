using System;
using System.Diagnostics;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using KBEngine;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using UniqueCream.MCSExtraTools.MoreTools;
using Random = UnityEngine.Random;
using UniqueCream.MCSExtraTools.Patch;
using BehaviorDesigner.Runtime.Tasks;
using GUIPackage;
using System.Linq;

namespace MCSxReverendInsanity
{
    [BepInPlugin(modName, "至尊仙窍", "1.0.0")]
    public class ReverendInsanityTalent : BaseUnityPlugin
    {
        const int buffId = 666001;
        const string deathdateKey = "DeathDate";
        static DateTime remainDate = new DateTime(11, 1, 1);
        const string modName = "me.jacklee.mcs.reverendinsanity.telent";

        public const float stealHPPercentage = 0.6f;
        public const float stealExpPercentage = 0.6f;
        public const float stealSpeedPercentage = 0.4f;
        public const float stealMindPercentage = 0.4f;
        public const float stealWudaoPercentage = 0.3f;

        static ManualLogSource logger;
        static String[] avatorType = { "人", "妖", "魔", "鬼" };
        static String[] jingjies = { "炼气", "筑基", "金丹", "元婴", "化神" };
        static int[] killCounter = { 120, 360, 720, 1440, 6000 };
        static Dictionary<string, string> WuDaosKeyValuePair = new Dictionary<string, string>()
        {
            { "1", "金之道" },
            { "2", "木之道" },
            { "3", "水之道" },
            { "4", "火之道" },
            { "5", "土之道" },
            { "6", "神之道" },
            { "7", "体道" },
            { "8", "剑道" },
            { "9", "气道" },
            { "10", "阵道" },
            { "21", "丹道" },
            { "22", "器道" }
        };

        void Start()
        {
            logger = base.Logger;
            Harmony.CreateAndPatchAll(typeof(ReverendInsanityTalent));
        }

        private static void ShowGameTip(string msg, PopTipIconType icontype = PopTipIconType.叹号)
        {
            UIPopTip.Inst.Pop(msg, icontype);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LoadGame), "LoadSave_Postfix")]
        public static void LoadSave_Postfix()
        {
            remainDate = DataTools.Instance.Load<DateTime>(modName, deathdateKey, Tools.instance.getPlayer().worldTimeMag.getNowTime().AddMonths(killCounter[getPlayerJingJie()]));
            if (Tools.instance.CheckHasTianFu(buffId) && !Tools.instance.getPlayer().taskMag._TaskData["Task"].HasField(buffId.ToString()))
            {
                addKillerTask(killCounter[getPlayerJingJie()]);
                SaveKillData(remainDate);
                ShowGameTip("至尊仙窍任务已开始", PopTipIconType.任务进度);
            }
        }

        static int getPlayerJingJie() => (Tools.instance.getPlayer().level - 1) / 3;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Avatar), "AddTime")]
        private static bool Avatar_AddTime_Prefix()
        {
            if (Tools.instance.CheckHasTianFu(buffId) && Tools.instance.getPlayer().worldTimeMag.getNowTime().ToString("yyyy/MM/dd") == "0001/01/01")
            {
                remainDate = Tools.instance.getPlayer().worldTimeMag.getNowTime().AddMonths(killCounter[getPlayerJingJie()]);
                SaveKillData(remainDate);
                ShowGameTip("至尊仙窍任务已开始", PopTipIconType.任务进度);
                addKillerTask(killCounter[getPlayerJingJie()]);
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Avatar), "AddTime")]
        private static void Avatar_AddTime_Postfix()
        {

            if (Tools.instance.CheckHasTianFu(buffId) && Tools.instance.getPlayer().worldTimeMag.getNowTime() > remainDate)
            {
                ShowGameTip("因长时间没杀害修士而亡", PopTipIconType.叹号);
                UIDeath.Inst.Show(DeathType.身死道消);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NPCDeath), "SetNpcDeath")]
        private static bool NPCDeath_SetNpcDeath_PrePatch(int npcId)
        {

            StackTrace stackTrace = new StackTrace();
            bool flag = stackTrace.GetFrame(2).GetMethod().ToString() == "Void Init()" && stackTrace.GetFrame(3).GetMethod().ToString() == "Void Awake()" && Tools.instance.CheckHasTianFu(buffId);
            if (flag)
            {
                //Log(NpcJieSuanManager.inst.GetNpcData(20329));
                JSONObject npc = NpcJieSuanManager.inst.GetNpcData(npcId);
                Avatar player = Tools.instance.getPlayer();
                //等級
                int npcLevel = npc["Level"].I;          
                //血量
                int npcHP = npc["HP"].I;
                //主動技
                List<int> npcSkillsId = npc["skills"].ToList();
                //被動技
                List<int> npcStaticSkillsKey = npc["staticSkills"].ToList();
                //經驗條
                int npcExp = npc["exp"].I;
                //悟道
                JSONObject npcWuDaos = npc["wuDaoJson"];
                //神識
                int npcShengShi = npc["shengShi"].I;
                //循速
                int npcSpeed = npc["dunSu"].I;

                remainDate = remainDate.AddMonths(killCounter[getPlayerJingJie()]);
                SaveKillData(remainDate);

                addKillerTask(killCounter[getPlayerJingJie()]);

                if (getPlayerJingJie() < (npcLevel - 1) / 3 && npcLevel - player.level > 1)
                {
                    if (Random.Range(0, 100) > 39)
                    {
                        ShowGameTip("以小吞大失败。 (60%失败率)", PopTipIconType.叹号);
                        return true;
                    }
                    else
                    {
                        ShowGameTip("以小吞大成功！", PopTipIconType.感悟);
                    }
                }
                //string getPlayerWuDaosSummary()
                //{
                //    string playerWudaoSummary = "";
                //    foreach (var keyvalue in WuDaosKeyValuePair)
                //    {
                //        playerWudaoSummary += "\t" + keyvalue.Value + ": " + player.WuDaoJson[keyvalue.Key]["ex"] + "\n";
                //    }
                //    return playerWudaoSummary;
                //}
                //logger.LogInfo($"\n==== NPC Info ====\nNPC ID: {npcId}\n等級: {npcLevel}\n" +
                //               $"境界: {npcJingJie}\n血量: {npcHP}\n神識: {npcShengShi}\nEXP: {npcExp}\n" +
                //               $"技能: {string.Join(", ", npcSkills.ToArray())}\n被動: {string.Join(", ", npcStaticSkills.ToArray())}");

                //logger.LogInfo($"\nPlayer Info:\nMAX HP:  {player.HP_Max}\nEXP: {player.exp}\n" +
                //               $"EXP Threshold: {jsonData.instance.LevelUpDataJsonData[string.Concat(player.level)]["MaxExp"]}\n" +
                //               $"Reach Caped?: {IsReachedExpThreshold()}\nLevel: {player.level}\nSpeed: {player.dunSu}\nMind: {player.shengShi}\n" +
                //               $"{getPlayerWuDaosSummary()}\n");

                //logger.LogInfo($"\nNPC Info:\nMAX HP:  {npcHP} (+{toInt(npcHP * stealHPPercentage)})\nEXP: {npcExp} (+{toInt(npcExp * stealExpPercentage)})\n" +
                //               $"Level: {npcLevel}\nSpeed: {npcSpeed} (+{toInt(npcSpeed * stealSpeedPercentage)})\n" +
                //               $"Mind: {npcShengShi} (+{toInt(npcShengShi * stealSpeedPercentage)})\n"); 




                int stealHP = (int)(npcHP * stealHPPercentage);
                player._HP_Max += stealHP;
                ShowGameTip($"已夺取{stealHP}血量", PopTipIconType.感悟);

                int stealExp = (int)(npcExp * stealExpPercentage);
                player.addEXP(stealExp);
                ShowGameTip($"已夺取{stealExp}修为", PopTipIconType.感悟);


                int stealShenShi = (int)(npcShengShi * stealMindPercentage);
                player.addShenShi(stealShenShi);

                int stealDunSu = (int)(npcSpeed * stealSpeedPercentage);
                player._dunSu += stealDunSu;

                         
                ShowGameTip($"已夺取{stealShenShi}神识", PopTipIconType.感悟);
                ShowGameTip($"已夺取{stealDunSu}循速", PopTipIconType.感悟);

                // Try steal wudaos
                foreach (var keyvalue in WuDaosKeyValuePair)
                {
                    //logger.LogInfo($"Player Wudao : {player.WuDaoJson[keyvalue.Key].GetField("ex")} + NPC : {npcWuDaos[keyvalue.Key]["exp"].I} * stealWudaoPercentage = {(int)(npcWuDaos[keyvalue.Key]["exp"].I * stealWudaoPercentage)}");
                    if (player.WuDaoJson.HasField(keyvalue.Key) && npcWuDaos.HasField(keyvalue.Key))
                        player.WuDaoJson[keyvalue.Key].SetField("ex", (int)(player.WuDaoJson[keyvalue.Key]["ex"].I + npcWuDaos[keyvalue.Key]["exp"].I * stealWudaoPercentage));
                }

                ShowGameTip($"已夺取对方{(int)(stealWudaoPercentage * 100)}%大道感悟", PopTipIconType.感悟);

                //logger.LogInfo($"\nPlayer Info:\nMAX HP:  {player.HP_Max}\nEXP: {player.exp}\n" +
                //               $"EXP Threshold: {jsonData.instance.LevelUpDataJsonData[string.Concat(player.level)]["MaxExp"]}\n" +
                //               $"Reach Caped?: {IsReachedExpThreshold()}\nLevel: {player.level}\nSpeed: {player.dunSu}\nMind: {player.shengShi}\n" +
                //               $"{getPlayerWuDaosSummary()}\n");

                // Key = The ONLY SINGLE skill ID
                // Skill ID = Skill ID

                // 取得此角色相應等級的技能的真實Key
                // Tools.instance.getSkillKeyByID(可重覆技能ID, Tools.instance.getPlayer())

                // jsonData.instance._skillJsonData[真實Key]

                // 取符名字
                // Tools.instance.getSkillName(真實Key)
                // 取符介紹
                // Tools.instance.getSkillText(真實Key)

                /*
                    主動技能 qingjiaotype
                    7 = 陣法-驅，領域    
                    6 = 魔修
                    5 = 千流岛技能
                    4 = 碎星島技能 (可學)
                 */

                // 被動: 
                // Tools.instance.getStaticSkillIDByKey(Static Skill ID, can get from NPC);
                // 名: Tools.instance.getStaticSkillName(Key)

                // 加神通: public void addHasSkillList(int SkillId)
                // 加功法: public void addHasStaticSkillList(int SkillId, int _level = 1)

                // Steal Skill ID!!! = [1,201,101,301,401,501,504]
                List<string> skillKeyList = GetAvaiableSkillsKeysByIDs(npcSkillsId);
                //logger.LogInfo("My skill Key : " + String.Join(", ", skillKeyList));
                skillKeyList = skillKeyList.Where(i => jsonData.instance.skillJsonData[i]["qingjiaotype"].I != 7).ToList();
                if (skillKeyList.Count > 0)
                {
                    string selectedKey = skillKeyList[Random.Range(0, skillKeyList.Count)];
                    ShowGameTip($"已夺取神通 {Tools.Code64(jsonData.instance.skillJsonData[selectedKey]["name"].str)}", PopTipIconType.感悟);
                    player.addHasSkillList(jsonData.instance.skillJsonData[selectedKey]["Skill_ID"].I);
                } else
                {
                    ShowGameTip("对方的神通你已全部习得", PopTipIconType.感悟);
                }

                //Static Skill KEY!!! = [5058,5243,5269,5117,5068]
                //JSONObject npc = NpcJieSuanManager.inst.GetNpcData(20550);
                //Log(npc["staticSkills"]);
                skillKeyList = GetAvaiableStaticSkillsKeysByKeys(npcStaticSkillsKey);
                //logger.LogInfo("My static Key : " + String.Join(", ", skillKeyList));           
                if (skillKeyList.Count > 0)
                {
                    string selectedKey = skillKeyList[Random.Range(0, skillKeyList.Count)];
                    ShowGameTip($"已夺取功法 {Tools.Code64(jsonData.instance.StaticSkillJsonData[selectedKey]["name"].str)}", PopTipIconType.感悟);
                    player.addHasStaticSkillList(jsonData.instance.StaticSkillJsonData[selectedKey]["Skill_ID"].I);
                }
                else
                {
                    ShowGameTip("对方的功法你已全部习得", PopTipIconType.感悟);
                }               

            }
            return true;
        }


        private static bool isReachThreshold()
        {
            Avatar player = Tools.instance.getPlayer();
            return player.exp >= (ulong)jsonData.instance.LevelUpDataJsonData[string.Concat(player.level)]["MaxExp"].n && player.level % 3 == 0;
        }
        private static void SaveKillData(DateTime date) => DataTools.Instance.Save<DateTime>(modName, deathdateKey, date);

        private static List<string> GetAvaiableSkillsKeysByIDs(List<int> originalSkillList)
        {
            List<string> skillList = new List<string>();
            foreach (int skillId in originalSkillList)
            {
                bool learnt = Tools.instance.getPlayer().hasSkillList.Where(i => i.itemId == skillId).Count() > 0;
                if (!learnt)
                    skillList.Add(Tools.instance.getSkillKeyByID(skillId, Tools.instance.getPlayer()).ToString());
                    
            }
            return skillList;
        }

        private static List<string> GetAvaiableStaticSkillsKeysByKeys(List<int> originalSkillList)
        {
            List<string> skillList = new List<string>();
            foreach (int skillkey in originalSkillList)
            {
                int skillid = jsonData.instance.StaticSkillJsonData[skillkey.ToString()]["Skill_ID"].I;
                bool learnt = Tools.instance.getPlayer().hasStaticSkillList.Where(i => i.itemId == skillid).Count() > 0;
                if (!learnt)
                    skillList.Add(skillkey.ToString());

            }
            return skillList;
        }

        private static void addKillerTask(int continueMonth)
        {
            int taskId = buffId;
            Avatar player = Tools.instance.getPlayer();
            TaskMag taskmag = player.taskMag;
            
            JSONObject jsonobject = new JSONObject();
            jsonobject.AddField("id", taskId);
            jsonobject.AddField("NowIndex", 1);
            JSONObject tmp = new JSONObject(JSONObject.Type.ARRAY);
            tmp.Add(1);
            jsonobject.AddField("AllIndex", tmp);
            jsonobject.AddField("disableTask", false);
            jsonobject.AddField("finishIndex", new JSONObject(JSONObject.Type.ARRAY));
            jsonobject.AddField("curTime", player.worldTimeMag.nowTime);
            jsonobject.AddField("continueTime", continueMonth);
            jsonobject.AddField("isComplete", false);
            if (jsonData.instance.TaskJsonData[string.Concat(taskId)]["EndTime"].str != "")
            {
                jsonobject.AddField("EndTime", jsonData.instance.TaskJsonData[string.Concat(taskId)]["EndTime"].str);
            }

            if (taskmag._TaskData["Task"].HasField(taskId.ToString()))
            {
                taskmag._TaskData["Task"].SetField(string.Concat(taskId), jsonobject);
            } else
            {
                taskmag._TaskData["Task"].AddField(string.Concat(taskId), jsonobject);
                taskmag.setTaskIndex(taskId, 1);
            }        
        }

    }
}
