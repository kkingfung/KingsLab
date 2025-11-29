using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Physics;
using UnityEditor;
using UnityEngine;
using Unity.RemoteConfig;
using System.IO;

namespace RandomTowerDefense.Info
{
    /// <summary>
    /// ステージ情報ID列挙型
    /// </summary>
    /// <remarks>UnityConsoleから直接情報取得する際に使用</remarks>
    public enum StageInfoID
    {
        EnumStageSize = 0,
        EnumWaveNum,
        EnumEnemyNum,
        EnumEnemyAttribute,
        EnumHpMax,
        EnumObstaclePercent,
        EnumResource,
        EnumSpawnSpeed,
    }

    /// <summary>
    /// ステージ情報静的クラス - ゲーム全体のステージ設定と敵配置データ管理
    ///
    /// 主な機能:
    /// - 4島（Easy、Normal、Hard、Ultra）のウェーブ設定管理
    /// - 動的敵強化システムとランダムボーナス敵配置
    /// - JSON設定読み込みとリモート設定統合
    /// - ステージ難易度曲線と敵タイプバランス調整
    /// - プロシージャル敵配置とカスタムステージ対応
    /// </summary>
    public static class StageInfoDetail
    {
        /// <summary>
        /// 島の数
        /// </summary>
        public static readonly int IslandNum = 4;

        /// <summary>
        /// ステージ情報
        /// </summary>
        public static StageAttr stageDetail;


        /// <summary>
        /// ステージ情報初期化メソッド
        /// </summary>
        /// <param name="useFile"></param>
        /// <param name="filepath"></param>
        public static void Init(bool useFile, string filepath)
        {
            UpdateCustomizedData();

            int currentIsland = PlayerPrefs.GetInt("IslandNow", 0);
            stageDetail = useFile ?
                InitializeStageInfoWithFile(currentIsland, filepath)
                : InitializeStageInfoWithoutFile(currentIsland);
        }

        /// <summary>
        /// ステージ情報を直接入力で保存
        /// </summary>
        /// <param name="infoID">ステージ情報ID</param>
        /// <param name="newInfo">ユーザー設定情報</param>
        public static void SaveDataInPrefs_DirectInput(int infoID, float newInfo)
        {
            switch ((StageInfoID)infoID)
            {
                case StageInfoID.EnumWaveNum:
                    PlayerPrefs.SetFloat("waveNum", newInfo);
                    break;
                case StageInfoID.EnumStageSize:
                    PlayerPrefs.SetFloat("stageSize", Mathf.Clamp(newInfo,
                        MinMapDepth * MinMapDepth, MaxMapDepth * MaxMapDepth));
                    break;
                case StageInfoID.EnumEnemyNum:
                    PlayerPrefs.SetFloat("enemyNum", newInfo);
                    break;
                case StageInfoID.EnumEnemyAttribute:
                    PlayerPrefs.SetFloat("enemyAttr", newInfo);
                    break;
                case StageInfoID.Enum_spawnSpeed:
                    PlayerPrefs.SetFloat("spawnSpeed", input);
                    break;
                case StageInfoID.EnumObstaclePercent:
                    PlayerPrefs.SetFloat("obstaclePercent", Mathf.Clamp(newInfo,
                        MinObstaclePercent, MaxObstaclePercent));
                    break;
                case StageInfoID.EnumHpMax:
                    PlayerPrefs.SetFloat("hpMax", newInfo);
                    break;
                case StageInfoID.EnumResource:
                    PlayerPrefs.SetFloat("resource", newInfo);
                    break;
            }
        }

        /// <summary>
        /// ステージ情報を変更して保存
        /// </summary>
        /// <param name="infoID">ステージ情報ID</param>
        /// <param name="chg">変更値</param>
        /// <returns></returns>
        public static float SaveDataInPrefs(int infoID, int chg)
        {
            float tempVal = 0;
            int currentID = GetNearestElement(infoID);
            switch ((StageInfoID)infoID)
            {
                case StageInfoID.EnumWaveNum:
                    tempVal = (currentID + chg + waveNumFactor.Length) % waveNumFactor.Length;
                    PlayerPrefs.SetFloat("waveNum", waveNumFactor[(int)tempVal]);
                    return waveNumFactor[(int)tempVal];
                case StageInfoID.EnumStageSize:
                    tempVal = (currentID + chg + stageSizeFactor.Length) % stageSizeFactor.Length;
                    PlayerPrefs.SetFloat("stageSize", stageSizeFactor[(int)tempVal]);
                    return stageSizeFactor[(int)tempVal];
                case StageInfoID.EnumEnemyNum:
                    tempVal = (currentID + chg + enemyNumFactor.Length) % enemyNumFactor.Length;
                    PlayerPrefs.SetFloat("enemyNum", enemyNumFactor[(int)tempVal]);
                    return enemyNumFactor[(int)tempVal];
                case StageInfoID.EnumEnemyAttribute:
                    tempVal = (currentID + chg + enemyAttributeFactor.Length) % enemyAttributeFactor.Length;
                    PlayerPrefs.SetFloat("enemyAttr", enemyAttributeFactor[(int)tempVal]);
                    return enemyAttributeFactor[(int)tempVal];
                case StageInfoID.EnumObstaclePercent:
                    tempVal = (currentID + chg + obstacleFactor.Length) % obstacleFactor.Length;
                    PlayerPrefs.SetFloat("obstaclePercent", obstacleFactor[(int)tempVal]);
                    return obstacleFactor[(int)tempVal];
                case StageInfoID.Enum_spawnSpeed:
                    tempVal = (currentID + chg + spawnSpeedFactor.Length) % spawnSpeedFactor.Length;
                    PlayerPrefs.SetFloat("spawnSpeed", spawnSpeedFactor[(int)tempVal]);
                    return spawnSpeedFactor[(int)tempVal];
                case StageInfoID.EnumHpMax:
                    tempVal = (currentID + chg + hpMaxFactor.Length) % hpMaxFactor.Length;
                    PlayerPrefs.SetFloat("hpMax", hpMaxFactor[(int)tempVal]);
                    return hpMaxFactor[(int)tempVal];
                case StageInfoID.EnumResource:
                    tempVal = (currentID + chg + resourceFactor.Length) % resourceFactor.Length;
                    PlayerPrefs.SetFloat("resource", resourceFactor[(int)tempVal]);
                    return resourceFactor[(int)tempVal];
            }

            return 0f;
        }

        /// <summary>
        /// 最も近い要素を取得
        /// </summary>
        /// <param name="infoID">ステージ情報ID</param>
        /// <returns>最も近い要素</returns>
        private static int GetNearestElement(int infoID)
        {
            int elementID = 0;
            float tempVal;
            switch ((StageInfoID)infoID)
            {
                case StageInfoID.EnumWaveNum:
                    tempVal = PlayerPrefs.GetFloat("waveNum", 1);
                    for (; elementID < waveNumFactor.Length; elementID++)
                    {
                        if (waveNumFactor[elementID] > tempVal) break;
                    }
                    break;
                case StageInfoID.EnumStageSize:
                    tempVal = PlayerPrefs.GetFloat("stageSize", 1);
                    for (; elementID < stageSizeFactor.Length; elementID++)
                    {
                        if (stageSizeFactor[elementID] > tempVal) break;
                    }
                    break;
                case StageInfoID.EnumEnemyNum:
                    tempVal = PlayerPrefs.GetFloat("enemyNum", 1);
                    for (; elementID < enemyNumFactor.Length; elementID++)
                    {
                        if (enemyNumFactor[elementID] > tempVal) break;
                    }
                    break;
                case StageInfoID.EnumEnemyAttribute:
                    tempVal = PlayerPrefs.GetFloat("enemyAttr", 1);
                    for (; elementID < enemyAttributeFactor.Length; elementID++)
                    {
                        if (enemyAttributeFactor[elementID] > tempVal) break;
                    }
                    break;
                case StageInfoID.EnumObstaclePercent:
                    tempVal = PlayerPrefs.GetFloat("obstaclePercent", 1);
                    for (; elementID < obstacleFactor.Length; elementID++)
                    {
                        if (obstacleFactor[elementID] > tempVal) break;
                    }
                    break;
                case StageInfoID.Enum_spawnSpeed:
                    tempVal = PlayerPrefs.GetFloat("spawnSpeed", 1);
                    for (; elementID < spawnSpeedFactor.Length; elementID++)
                    {
                        if (spawnSpeedFactor[elementID] > tempVal) break;
                    }
                    break;
                case StageInfoID.EnumHpMax:
                    tempVal = PlayerPrefs.GetFloat("hpMax", 10);
                    for (; elementID < hpMaxFactor.Length; elementID++)
                    {
                        if (hpMaxFactor[elementID] > tempVal) break;
                    }
                    break;
                case StageInfoID.EnumResource:
                    tempVal = PlayerPrefs.GetFloat("resource", 1);
                    for (; elementID < resourceFactor.Length; elementID++)
                    {
                        if (resourceFactor[elementID] > tempVal) break;
                    }
                    break;
            }
            return Mathf.Max(elementID - 1, 0);
        }

        /// <summary>
        /// ファイルからステージ情報を初期化
        /// </summary>
        /// <param name="waveNum">ウェブ数</param>
        /// <param name="filepath">ファイルパス</param>
        /// <returns>ステージのウェブ情報リスト</returns>
        private static WaveAttr[] PrepareStageInfoWithFile(int waveNum, string filepath)
        {
            var _inpStm = new StreamReader(filepath);
            var _waveArray = new WaveAttr[waveNum];
            var _detail = new List<WaveDetail>();

            while (!_inpStm.EndOfStream)
            {
                string inp_ln = _inpStm.ReadLine();
                string[] seperateInfo = inp_ln.Split(':');

                if (seperateInfo.Length == IslandNum)
                {
                    var detail = new EnemyDetail(
                        int.Parse(seperateInfo[0]), int.Parse(seperateInfo[1]));
                    _detail.Add(detail, int.Parse(seperateInfo[2]), seperateInfo[3]);
                }
            }

            _inpStm.Close();

            int j = 0;

            for (int i = 0; i < waveNum; ++i)
            {
                List<WaveDetail> detailPerWave = new List<WaveDetail>();
                for (; j < _detail.Count && _detail[j].waveID <= i + 1; ++j)
                {
                    if (_detail[j].waveID == i + 1)
                    {
                        detailPerWave.Add(_detail[j]);
                    }
                }
                _waveArray[i] = new WaveAttr(detailPerWave);
            }

            return _waveArray;
        }

        /// <summary>
        /// ステージ情報を初期化（ファイル使用時）
        /// </summary> 
        /// <param name="islandId">島ID</param>
        /// <param name="filepath">ファイルパス</param>
        /// <returns>ステージ情報</returns>
        static StageAttr InitializeStageInfoWithFile(int islandId, string filepath) => islandId switch
        {
            0 => new StageAttr(EasyStageWaveNum,
                        PrepareStageInfoByFile(EasyStageWaveNum,
                            filepath + "/EasyStageInfoList.txt")),
            1 => new StageAttr(NormalStageWaveNum,
                        PrepareStageInfoByFile(NormalStageWaveNum,
                            filepath + "/NormalStageInfoList.txt")),
            2 => new StageAttr(HardStageWaveNum,
                        PrepareStageInfoByFile(HardStageWaveNum,
                            filepath + "/HardStageInfoList.txt")),
            3 => new StageAttr((int)PlayerPrefs.GetFloat("waveNum", 10),
                        PrepareCustomStageInfo((int)PlayerPrefs.GetFloat("waveNum", 10))),
            _ => throw new System.Exception("Invalid island ID"),
        };

        /// <summary>
        /// ステージ情報を初期化（ファイル未使用時）
        /// </summary>
        /// <param name="islandId">島ID</param>
        /// <returns>ステージ情報</returns>
        static StageAttr InitializeStageInfoWithoutFile(int islandId) => islandId switch
        {
            0 => stageDetail = new StageAttr(DefaultStageInfos.EasyStageWaveNum,
                        PrepareEasyStageInfo(DefaultStageInfos.EasyStageWaveNum)),
            1 => stageDetail = new StageAttr(DefaultStageInfos.NormalStageWaveNum,
                        PrepareNormalStageInfo(DefaultStageInfos.NormalStageWaveNum)),
            2 => stageDetail = new StageAttr(DefaultStageInfos.HardStageWaveNum,
                        PrepareHardStageInfo(DefaultStageInfos.HardStageWaveNum)),
            3 => stageDetail = new StageAttr((int)PlayerPrefs.GetFloat("waveNum", 10),
                        PrepareCustomStageInfo((int)PlayerPrefs.GetFloat("waveNum", 10))),
            _ => throw new System.Exception("Invalid island ID"),
        };
    }

    /// <summary>
    /// ステージ情報静的クラス - ゲーム全体のステージ設定と敵配置データ管理（Consoleにつながらない際に使用）
    /// </summary>
    public static class DefaultStageInfos
    {
        /// <summary>
        /// イーシー各難易度ステージのウェーブ数
        /// </summary>
        public static readonly int EasyStageWaveNum = 5;

        /// <summary>
        /// ノーマルステージのウェーブ数
        /// </summary>
        public static readonly int NormalStageWaveNum = 15;
        /// <summary>
        /// ハードステージのウェーブ数
        /// </summary>
        public static readonly int HardStageWaveNum = 30;

        /// <summary>
        /// マップの最大深度
        /// </summary>
        public static readonly int MaxMapDepth = 8;

        /// <summary>
        /// マップの最小深度
        /// </summary>
        public static readonly int MinMapDepth = 8;
        /// <summary>
        /// 障害物の最大出現率
        /// </summary>
        public static readonly float MaxObstaclePercent = 1f;

        /// <summary>
        /// 障害物の最小出現率
        /// </summary>
        public static readonly float MinObstaclePercent = 0.3f;

        /// <summary>
        /// 乱数生成器
        /// </summary>
        public static readonly System.Random Prng = new System.Random((int)Time.time);

        /// <summary>
        /// モンスターカテゴリ別出現タイプリスト(グループ 0)
        /// </summary>
        public static readonly string[] MonsterCat0 = {
            "Slime","Mushroom","PhoenixChick","TurtleShell"
        };

        /// <summary>
        /// モンスターカテゴリ別出現タイプリスト(グループ 1)
        /// </summary>
        public static readonly string[] MonsterCat1 = {
            "Footman","Grunt","TurtleShell","RockCritter",
            "FootmanS","GruntS","Skeleton",
            "StoneMonster"
        };

        /// <summary>
        /// モンスターカテゴリ別出現タイプリスト(グループ 2)
        /// </summary>
        public static readonly string[] MonsterCat2 = {
            "FootmanS","GruntS","SpiderGhost","PigChef",
            "SkeletonArmed","Golem","GolemS",
            "FreeLich",  "FreeLichS",  "Bull",
            "StoneMonster"
        };

        /// <summary>
        /// モンスターカテゴリ別出現タイプリスト(グループ 3)
        /// </summary>
        /// <remarks>UnityConsoleから直接情報取得する際に使用</remarks>
        public static readonly string[] MonsterCat3 = {
            "FootmanS", "GruntS", "SpiderGhost",
            "SkeletonArmed","GolemS", "FreeLichS",
            "Bull", "Dragon"
        };

        /// <summary>
        /// 簡単ステージ情報を準備
        /// </summary>
        /// <param name="waveNum">ウェブ数</param>
        /// <returns>ステージのウェブ情報リスト</returns>
        private static WaveAttr[] PrepareEasyStageInfo(int waveNum)
        {
            WaveAttr[] waveArray = new WaveAttr[waveNum];

            List<WaveDetail> detail = new List<WaveDetail>();
            int waveIDCnt = 1;
            detail.Add(new EnemyDetail(waveIDCnt++, 5, 1, "Slime"));
            detail.Add(new EnemyDetail(waveIDCnt++, 5, 1, "Mushroom"));
            detail.Add(new EnemyDetail(waveIDCnt++, 10, 1, "Slime"));
            detail.Add(new EnemyDetail(waveIDCnt++, 10, 1, "TurtleShell"));
            detail.Add(new EnemyDetail(waveIDCnt++, 1, 1, "StoneMonster"));

            int j = 0;

            for (int i = 0; i < waveNum; ++i)
            {
                List<WaveDetail> detailPerWave = new List<WaveDetail>();
                for (; j < detail.Count && detail[j].waveID <= i + 1; ++j)
                {
                    if (detail[j].waveID == i + 1)
                    {
                        detailPerWave.Add(detail[j]);
                    }
                }
                waveArray[i] = new WaveAttr(detailPerWave);
            }

            return waveArray;
        }

        /// <summary>
        /// 普通ステージ情報を準備
        /// </summary>
        /// <param name="waveNum">ウェブ数</param>
        /// <returns>ステージのウェブ情報リスト</returns>
        private static WaveAttr[] PrepareNormalStageInfo(int waveNum)
        {
            WaveAttr[] waveArray = new WaveAttr[waveNum];

            List<WaveDetail> detail = new List<WaveDetail>();
            int waveIDCnt = 1;
            detail.Add(new EnemyDetail(waveIDCnt++, 5, 1, "Slime"));
            detail.Add(new EnemyDetail(waveIDCnt++, 5, 1, "Mushroom"));
            detail.Add(new EnemyDetail(waveIDCnt++, 8, 2, "Slime"));
            detail.Add(new EnemyDetail(waveIDCnt++, 8, 1, "Mushroom"));
            detail.Add(new EnemyDetail(waveIDCnt++, 10, 0, "TurtleShell"));

            detail.Add(new EnemyDetail(waveIDCnt++, 10, 1, "Footman"));
            detail.Add(new EnemyDetail(waveIDCnt++, 10, 1, "Slime"));
            detail.Add(new EnemyDetail(waveIDCnt++, 15, 2, "Grunt"));
            detail.Add(new EnemyDetail(waveIDCnt++, 15, 2, "Skeleton"));
            detail.Add(new EnemyDetail(waveIDCnt++, 15, 0, "Footman"));

            detail.Add(new EnemyDetail(waveIDCnt++, 15, 0, "FootmanS"));
            detail.Add(new EnemyDetail(waveIDCnt++, 20, 1, "Skeleton"));
            detail.Add(new EnemyDetail(waveIDCnt++, 10, 0, "GruntS"));
            detail.Add(new EnemyDetail(waveIDCnt++, 15, 2, "TurtleShell"));
            detail.Add(new WaveDetail(waveIDCnt, 1, 0, "Bull"));
            detail.Add(new WaveDetail(waveIDCnt, 1, 2, "Bull"));
            detail.Add(new WaveDetail(waveIDCnt, 1, 1, "Bull"));

            int j = 0;

            for (int i = 0; i < waveNum; ++i)
            {
                List<WaveDetail> detailPerWave = new List<WaveDetail>();
                for (; j < detail.Count && detail[j].waveID <= i + 1; ++j)
                {
                    if (detail[j].waveID == i + 1)
                    {
                        detailPerWave.Add(detail[j]);
                    }
                }
                waveArray[i] = new WaveAttr(detailPerWave);
            }
            return waveArray;
        }

        /// <summary>
        /// 難しいステージ情報を準備
        /// </summary>
        /// <param name="waveNum">ウェブ数</param>
        /// <returns>ステージのウェブ情報リスト</returns>
        private static WaveAttr[] PrepareHardStageInfo(int waveNum)
        {
            WaveAttr[] waveArray = new WaveAttr[waveNum];
            List<WaveDetail> detail = new List<WaveDetail>();
            int waveIDCnt = 1;
            detail.Add(new WaveDetail(waveIDCnt++, 5, 1, "Slime"));
            detail.Add(new WaveDetail(waveIDCnt++, 5, 1, "Mushroom"));
            detail.Add(new WaveDetail(waveIDCnt++, 8, 2, "TurtleShell"));
            detail.Add(new WaveDetail(waveIDCnt++, 8, 2, "Footman"));
            detail.Add(new WaveDetail(waveIDCnt++, 8, 0, "Mushroom"));

            detail.Add(new WaveDetail(waveIDCnt++, 10, 0, "Grunt"));
            detail.Add(new WaveDetail(waveIDCnt++, 12, 1, "Skeleton"));
            detail.Add(new WaveDetail(waveIDCnt++, 12, 2, "Slime"));
            detail.Add(new WaveDetail(waveIDCnt++, 12, 0, "Grunt"));
            detail.Add(new WaveDetail(waveIDCnt++, 15, 0, "TurtleShell"));

            detail.Add(new WaveDetail(waveIDCnt++, 15, 1, "Mushroom"));
            detail.Add(new WaveDetail(waveIDCnt++, 15, 1, "Grunt"));
            detail.Add(new WaveDetail(waveIDCnt++, 15, 2, "Skeleton"));
            detail.Add(new WaveDetail(waveIDCnt++, 15, 2, "Footman"));
            detail.Add(new WaveDetail(waveIDCnt, 2, 2, "StoneMonster"));
            detail.Add(new WaveDetail(waveIDCnt, 2, 0, "StoneMonster"));
            detail.Add(new WaveDetail(waveIDCnt++, 1, 1, "StoneMonster"));

            detail.Add(new WaveDetail(waveIDCnt++, 10, 2, "SpiderGhost"));
            detail.Add(new WaveDetail(waveIDCnt++, 10, 1, "Golem"));
            detail.Add(new WaveDetail(waveIDCnt++, 15, 1, "SkeletonArmed"));
            detail.Add(new WaveDetail(waveIDCnt++, 15, 1, "FreeLich"));
            detail.Add(new WaveDetail(waveIDCnt++, 15, 0, "SpiderGhost"));

            detail.Add(new WaveDetail(waveIDCnt++, 20, 0, "Golem"));
            detail.Add(new WaveDetail(waveIDCnt++, 20, 2, "SkeletonArmed"));
            detail.Add(new WaveDetail(waveIDCnt++, 25, 2, "FreeLich"));
            detail.Add(new WaveDetail(waveIDCnt++, 25, 1, "Slime"));
            detail.Add(new WaveDetail(waveIDCnt, 3, 0, "Bull"));
            detail.Add(new WaveDetail(waveIDCnt++, 3, 2, "Bull"));

            detail.Add(new WaveDetail(waveIDCnt++, 25, 1, "FootmanS"));
            detail.Add(new WaveDetail(waveIDCnt++, 25, 1, "SpiderGhost"));
            detail.Add(new WaveDetail(waveIDCnt++, 20, 0, "GolemS"));
            detail.Add(new WaveDetail(waveIDCnt++, 20, 2, "FreeLichS"));
            detail.Add(new WaveDetail(waveIDCnt++, 1, 1, "Dragon"));

            int j = 0;

            for (int i = 0; i < waveNum; ++i)
            {
                List<WaveDetail> detailPerWave = new List<WaveDetail>();
                for (; j < detail.Count && detail[j].waveID <= i + 1; ++j)
                {
                    if (detail[j].waveID == i + 1)
                    {
                        detailPerWave.Add(detail[j]);
                    }
                }
                waveArray[i] = new WaveAttr(detailPerWave);
            }
            return waveArray;
        }

        /// <summary>
        /// カスタムステージ情報を準備
        /// </summary>
        /// <param name="waveNum">ウェブ数</param>
        /// <returns>ステージのウェブ情報リスト</returns>
        private static WaveAttr[] PrepareCustomStageInfo(int waveNum)
        {
            var waveNumEx = (int)PlayerPrefs.GetFloat("waveNum", 1);
            var stageSizeEx = (int)PlayerPrefs.GetFloat("stageSize", 64);
            var enemyNumEx = (float)PlayerPrefs.GetFloat("enemyNum", 1);
            var enemyAttributeEx = (float)PlayerPrefs.GetFloat("enemyAttr", 1);
            var obstacleEx = (float)PlayerPrefs.GetFloat("obstaclePercent", 1);
            var spawnSpeedEx = (float)PlayerPrefs.GetFloat("spawnSpeed", 5);
            var hpMaxEx = (int)PlayerPrefs.GetFloat("hpMax", 1);
            var resourceEx = (float)PlayerPrefs.GetFloat("resource", 1);

            // NOTE: 動かすための仮データ（最終的にConsole経由デ設定する）
            var _waveArray = new WaveAttr[waveNum];
            var _detail = new List<WaveDetail>();
            for (int k = 1; k < waveNum; k++)
            {
                if (k < 10 - 1)
                {
                    _detail.Add(new EnemyDetail(k, 8 * (int)enemyNumEx,
                        Prng.Next(0, 3), DefaultStageInfos.MonsterCat0[Prng.Next() % DefaultStageInfos.MonsterCat0.Length]));
                }
                else if (k < 35 - 1)
                {
                    _detail.Add(new EnemyDetail(k, Prng.Next(5, 10) * (int)enemyNumEx,
                        Prng.Next(0, 3), DefaultStageInfos.MonsterCat1[Prng.Next(0, DefaultStageInfos.MonsterCat1.Length)]));
                    _detail.Add(new EnemyDetail(k, Prng.Next(5, 10) * (int)enemyNumEx,
                        Prng.Next(0, 3), DefaultStageInfos.MonsterCat1[Prng.Next(0, DefaultStageInfos.MonsterCat1.Length)]));
                }
                else if (k < 50 - 1)
                {
                    _detail.Add(new EnemyDetail(k, Prng.Next(10, 15) * (int)enemyNumEx,
                        Prng.Next(0, 3), DefaultStageInfos.MonsterCat2[Prng.Next(0, DefaultStageInfos.MonsterCat2.Length)]));
                    _detail.Add(new EnemyDetail(k, Prng.Next(10, 15) * (int)enemyNumEx,
                        Prng.Next(0, 3), DefaultStageInfos.MonsterCat2[Prng.Next(0, DefaultStageInfos.MonsterCat2.Length)]));
                    _detail.Add(new EnemyDetail(k, Prng.Next(10, 15) * (int)enemyNumEx,
                        Prng.Next(0, 3), DefaultStageInfos.MonsterCat2[Prng.Next(0, DefaultStageInfos.MonsterCat2.Length)]));
                }
                else
                {
                    if (k == 49)
                        _detail.Add(new WaveDetail(k, 3, 1, "RobotSphere"));
                    else
                    {
                        switch (k % 10)
                        {
                            case 9:
                                if (Prng.Next(0, 5) == 0)
                                    _detail.Add(new EnemyDetail(k, 1 * (int)enemyNumEx, 1, "AttackBot"));
                                else
                                {
                                    _detail.Add(new EnemyDetail(k, Prng.Next(1, 3) * (int)enemyNumEx,
                                        Prng.Next() % 3, "RobotSphere"));
                                    _detail.Add(new EnemyDetail(k, Prng.Next(1, 3) * (int)enemyNumEx,
                                        Prng.Next() % 3, "RobotSphere"));
                                    _detail.Add(new EnemyDetail(k, Prng.Next(1, 3) * (int)enemyNumEx,
                                        Prng.Next() % 3, "RobotSphere"));
                                }
                                break;
                            default:
                                _detail.Add(new EnemyDetail(k, Prng.Next(5, 10) * (int)StageInfoDetail.enemyNumEx,
                                    Prng.Next(0, 3), DefaultStageInfos.MonsterCat3[Prng.Next(0, DefaultStageInfos.MonsterCat3.Length)]));
                                _detail.Add(new EnemyDetail(k, Prng.Next(5, 10) * (int)StageInfoDetail.enemyNumEx,
                                    Prng.Next(0, 3), DefaultStageInfos.MonsterCat3[Prng.Next(0, DefaultStageInfos.MonsterCat3.Length)]));
                                _detail.Add(new EnemyDetail(k, Prng.Next(5, 10) * (int)StageInfoDetail.enemyNumEx,
                                    Prng.Next(0, 3), DefaultStageInfos.MonsterCat3[Prng.Next(0, DefaultStageInfos.MonsterCat3.Length)]));
                                break;
                        }

                    }
                }
            }

            int j = 0;

            for (int i = 0; i < waveNum; ++i)
            {
                List<WaveDetail> detailPerWave = new List<WaveDetail>();
                for (; j < _detail.Count && _detail[j].waveID <= i + 1; ++j)
                {
                    if (_detail[j].waveID == i + 1)
                    {
                        detailPerWave.Add(_detail[j]);
                    }
                }
                _waveArray[i] = new WaveAttr(detailPerWave);
            }
            return _waveArray;
        }
    }
}
