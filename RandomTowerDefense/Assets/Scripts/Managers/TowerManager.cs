using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.MapGenerator;
using RandomTowerDefense.Units;
using RandomTowerDefense.Info;

namespace RandomTowerDefense.Managers.System
{
    /// <summary>
    /// タワーの建設、マージ、売却、UI更新を含むすべてのタワー操作を管理
    /// ランダムなタワータイプの割り当てとランクを通じたタワーの進行を処理
    /// DOTSスポーナーシステムとビジュアルエフェクト管理と統合
    /// 4つのタワータイプをサポート: Nightmare、SoulEater、TerrorBringer、Usurper
    /// </summary>
    public class TowerManager : MonoBehaviour
    {
        #region Constants
        private readonly int NumReqToMerge = 3;
        private readonly int MonsterColorNumber = 4;
        private readonly int NumTowerType = 4;
        #endregion

        #region Tower Prefabs
        [Header("Tower Settings")]
        public GameObject TowerBuild;
        public GameObject TowerLevelUp;
        public GameObject TowerDisappear;
        public GameObject TowerSell;

        [Header("TowerAura Settings")]
        public GameObject TowerNightmareAura;
        public GameObject TowerSoulEaterAura;
        public GameObject TowerTerrorBringerAura;
        public GameObject TowerUsurperAura;
        #endregion

        #region UI Components
        [Header("TowerInfo Settings")]
        public List<GameObject> TargetInfo;
        private List<Text> targetInfoText;
        public List<Slider> TargetInfoSlider;
        #endregion

        #region Manager References
        public StageManager stageManager;
        public ResourceManager resourceManager;
        public FilledMapGenerator filledMapGenerator;
        public TowerSpawner towerSpawner;
        public CastleSpawner castleSpawner;
        public AudioManager audioManager;
        public AttackSpawner attackSpawner;
        public EffectSpawner effectManager;
        public DebugManager debugManager;
        public BonusChecker bonusChecker;
        public UpgradesManager upgradesManager;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// タワー情報テキストコンポーネントを初期化
        /// </summary>
        void Start()
        {
            targetInfoText = new List<Text>();
            foreach (GameObject i in TargetInfo)
            {
                targetInfoText.Add(i.GetComponent<Text>());
            }
        }

        /// <summary>
        /// 毎フレーム呼び出されるUnity更新メソッド（現在未使用）
        /// </summary>
        void Update()
        {

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 指定したターゲットのUI情報を更新
        /// </summary>
        /// <param name="target">情報を表示するターゲット（nullの場合はUIを非表示）</param>
        public void UpdateInfo(GameObject target)
        {
            for (int i = 0; i < TargetInfo.Count; ++i)
            {
                TargetInfo[i].SetActive(target != null);
                foreach (Slider j in TargetInfoSlider)
                {
                    j.transform.gameObject.SetActive(target != null);
                }
                if (target == null)
                {
                    continue;
                }

                if (targetInfoText[i])
                {
                    Tower towerinfo = target.GetComponent<Tower>();
                    targetInfoText[i].text = "Rank" + (towerinfo.IsAtMaxRank() ? "MAX" : towerinfo.rank.ToString())
                        + " Lv" + (towerinfo.IsAtMaxLevel() ? "MAX" : towerinfo.level.ToString());

                    targetInfoText[i].color = TowerTypeHandler.GetTowerColor(towerinfo.type);
                    foreach (Slider j in TargetInfoSlider)
                    {
                        j.maxValue = towerinfo.GetRequiredExp();
                        j.value = Mathf.Min(towerinfo.exp, j.maxValue);
                    }
                }
            }
        }

        /// <summary>
        /// 指定した柱の位置にタワーを建設
        /// </summary>
        /// <param name="pillar">タワーを建設する柱</param>
        /// <param name="rank">タワーのランク（デフォルト: 1）</param>
        public void BuildTower(GameObject pillar, int rank = 1)
        {
            if (pillar == null)
            {
                return;
            }
            if (rank == 1 && filledMapGenerator.ChkPillarStatusEmpty(pillar) == false)
            {
                return;
            }

            Vector3 location = pillar.transform.position + Vector3.up * filledMapGenerator.UpdatePillarStatus(pillar);
            filledMapGenerator.UpdatePillarStatus(pillar, BuildTower(pillar, location, rank) ? 1 : 0);
        }

        /// <summary>
        /// 指定した位置にタワーを建設
        /// </summary>
        /// <param name="pillar">タワーを建設する柱</param>
        /// <param name="location">建設位置</param>
        /// <param name="rank">タワーのランク（デフォルト: 1）</param>
        /// <returns>建設が成功した場合true</returns>
        public bool BuildTower(GameObject pillar, Vector3 location, int rank = 1)
        {
            GameObject tower;
            int[] entityIDList;
            if (resourceManager.ChkAndBuild(rank) == false)
            {
                return false;
            }

            {
                Tower script;

                var towerType = (TowerInfo.TowerInfoID)DefaultStageInfos.Prng.Next(0, NumTowerType);
                int spawnIndex = TowerTypeHandler.GetTowerSpawnIndex(towerType, rank, MonsterColorNumber);
                entityIDList = towerSpawner.Spawn(spawnIndex, location, castleSpawner.castle.transform.position);
                tower = towerSpawner.GameObjects[entityIDList[0]];
                script = tower.GetComponent<Tower>();
                script.LinkingManagers(stageManager, towerSpawner, audioManager, attackSpawner, filledMapGenerator, resourceManager, upgradesManager, bonusChecker, debugManager);
                var aura = TowerTypeHandler.GetTowerAura(towerType, TowerNightmareAura, TowerSoulEaterAura, TowerTerrorBringerAura, TowerUsurperAura);
                script.NewTower(entityIDList[0], towerSpawner, pillar, TowerLevelUp, aura, towerType, 1, rank);
                tower.transform.localScale = 0.1f * new Vector3(1, 1, 1);
                if (bonusChecker)
                {
                    bonusChecker.TowerNewlyBuilt = (int)towerType;
                }
            }
            effectManager.Spawn(0, location);
            return true;
        }

        /// <summary>
        /// 指定したタワーを他の同じタイプのタワーとマージ
        /// </summary>
        /// <param name="targetedTower">マージの対象となるタワー</param>
        /// <returns>マージが成功した場合true</returns>
        public bool MergeTower(GameObject targetedTower)
        {

            Tower targetedTowerScript = targetedTower.GetComponent<Tower>();
            // マージ用に同じタイプとランクの最大レベルのタワーを探す
            List<GameObject> candidateList = new List<GameObject>();
            List<GameObject> tempList;
            int count = 0;
            tempList = TowerTypeHandler.GetTowerListByTypeAndRank(towerSpawner, targetedTowerScript.type, targetedTowerScript.rank);

            tempList.Remove(targetedTower);
            while (tempList.Count > 0)
            {
                GameObject chkTarget = tempList[DefaultStageInfos.Prng.Next(0, tempList.Count)];
                tempList.Remove(chkTarget);
                if (chkTarget.activeSelf)
                {
                    Tower chkTowerScript = chkTarget.GetComponent<Tower>();
                    if (chkTowerScript.CanLevelUp())
                    {
                        candidateList.Add(chkTarget);
                        count++;
                    }
                }
            }

            if (count < NumReqToMerge - 1)
            {
                return false;
            }

            count = NumReqToMerge - 1;

            // 選択したタワーを削除し、マージエフェクトを作成
            while (count-- > 0)
            {
                GameObject candidate = candidateList[DefaultStageInfos.Prng.Next(0, candidateList.Count)];
                candidateList.Remove(candidate);

                GameObject temp = effectManager.Spawn(3, candidate.transform.position);
                VisualEffect tempVFX = temp.GetComponent<VisualEffect>();
                Tower candidateTowerScript = candidate.GetComponent<Tower>();
                if (tempVFX != null)
                {
                    tempVFX.SetVector4("MainColor", TowerTypeHandler.GetTowerVFXColor(candidateTowerScript.type));
                    tempVFX.SetVector3("TargetLocation", targetedTower.transform.position - candidate.transform.position);
                }
                RemoveTowerFromList(candidate);
            }
            RemoveTowerFromList(targetedTower);

            // 新しいランクのアップグレードされたタワーを建設
            BuildTower(targetedTowerScript.pillar, targetedTower.transform.position, targetedTowerScript.rank + 1);

            return true;
        }

        /// <summary>
        /// 指定したタワーをリストから削除し破棄
        /// </summary>
        /// <param name="targetedTower">削除するタワー</param>
        public void RemoveTowerFromList(GameObject targetedTower)
        {
            Tower targetedTowerScript = targetedTower.GetComponent<Tower>();
            targetedTowerScript.Destroy();
        }

        /// <summary>
        /// 指定したタワーを売却しリソースを取得
        /// </summary>
        /// <param name="targetedTower">売却するタワー</param>
        public void SellTower(Tower targetedTower)
        {
            if (resourceManager.SellTower(targetedTower))
            {
                filledMapGenerator.UpdatePillarStatus(targetedTower.gameObject, 0);
                effectManager.Spawn(5, targetedTower.transform.position);
                RemoveTowerFromList(targetedTower.gameObject);
            }
            #endregion
        }
    }
}
