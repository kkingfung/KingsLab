# コーディング規約ガイド

## 概要
本ガイドは、すべてのシステムが一貫したパターンに従い、保守性とパフォーマンスを確保することを目的としています。

═══════════════════════════════════════════════════════════════════════════════════════

##  1. 命名規則

### クラスとインターフェイス
```csharp
// ✅ CORRECT: PascalCase かつ意味が明確な名前
public class EquipmentManager : MonoBehaviour
public class SocialFeaturesManager : MonoBehaviour
public interface IResourceManager : IDisposable
public interface ITownManager

// ❌ INCORRECT: 名前が不明瞭、または大文字小文字の規則が間違っている
public class equipmentmgr : MonoBehaviour
public class SocialMgr : MonoBehaviour
public interface resourceManager
```

### メソッドとプロパティ
```csharp
// ✅ CORRECT: PascalCase、動作＋対象のパターンで明確
public void InitializeEquipmentSystem(EquipmentDatabase database)
public bool CanAffordBuilding(BuildingConfig config)
public TownResources GetCurrentResources()
public List<Monster> GetTownMonsters()

// ❌ INCORRECT: camelCase や意味の曖昧な名前
public void initEquipment(EquipmentDatabase db)
public bool canAfford(BuildingConfig cfg)
public TownResources resources()
```

### フィールドと変数
```csharp
// ✅ CORRECT: rivateはcamelCase＋接頭辞"_"、publicはPascalCase
[SerializeField] private EquipmentDatabase equipmentDatabase;
[SerializeField] private bool enableAutoDocumentation = true;
private Dictionary<string, PlayerWallet> _playerWallets = new();
private List<BreedingExperiment> _experiments = new();
public float Happiness { get; set; }

// ❌ INCORRECT: 命名規則が一貫していない
private EquipmentDatabase equipment_database;
private bool EnableAutoDocumentation = true;
private Dictionary<string, PlayerWallet> playerWallets = new();
```

### 定数と列挙体
```csharp
// ✅ CORRECT: 定数は UPPER_CASE、enum は PascalCase
public const float MAX_HAPPINESS = 1.0f;
public const int DEFAULT_POPULATION_LIMIT = 100;

public enum ActivityType { Racing, Combat, Puzzle, Strategy, Music }
public enum BuildingType { BreedingCenter, TrainingGrounds, ResearchLab }
public enum EquipmentRarity { Common, Uncommon, Rare, Epic, Legendary }

// ❌ INCORRECT: 大文字小文字の規則が誤っている
public const float maxHappiness = 1.0f;
public enum activityType { racing, combat, puzzle }
```

═══════════════════════════════════════════════════════════════════════════════════════

## 2. ネームスペース構造

### ネームスペース階層
```csharp
// ✅ CORRECT: 階層構造を意識した整理
namespace Laboratory.Core.MonsterTown          // 町システムのコア機能
namespace Laboratory.Core.Equipment           // 装備システム
namespace Laboratory.Core.Economy            // 経済システム
namespace Laboratory.Core.Social             // ソーシャル機能
namespace Laboratory.Core.Education          // 学習・教育要素
namespace Laboratory.Core.Discovery          // 発見・実績システム
namespace Laboratory.Core.Integration        // システム統合
namespace Laboratory.Core.Bootstrap          // 初期化処理

// ❌ INCORRECT: 平坦で一貫性のないネームスペース
namespace MonsterTown
namespace Equipment
namespace Economy
```

═══════════════════════════════════════════════════════════════════════════════════════

## 3. ドキュメンテーション規約

### クラスのドキュメント
```csharp
/// <summary>
/// 装備管理クラス — モンスターの装備に関する全てのメカニクスを処理します
///
/// 主な機能:
/// - 装備ボーナスはモンスターのアクティビティ性能に直接影響
/// - Common〜Legendary の5段階レアリティ
/// - アクティビティ別ボーナス（例：レース用装備はレース性能向上）
///
/// - セット装備ボーナスのサポート
/// - アップグレードやクラフトによる装備成長
/// - ScriptableObject によるデザイナー向け設定
/// </summary>
public class EquipmentManager : MonoBehaviour
```

### メソッドのドキュメント
```csharp
/// <summary>
/// モンスターに装備アイテムを付与します
/// </summary>
/// <param name="monster">装備対象のモンスター</param>
/// <param name="itemId">装備するアイテムのID</param>
/// <returns>装備が成功した場合 true、失敗した場合 false</returns>
public bool EquipItem(Monster monster, string itemId)
```

### インラインコメント
```csharp
// このアクティビティタイプ専用の装備ボーナスを計算する
var activityBonus = CalculateActivityBonus(equipment, activityType);

// レアリティに応じた倍率を加算する
var finalBonus = baseBonus * GetRarityMultiplier(equipment.Rarity);

// NOTE: 提案書の「クロスアクティビティボーナス」要件を満たす処理
var crossActivityBonus = CalculateCrossActivityBonus(monster, activityType);
```

═══════════════════════════════════════════════════════════════════════════════════════

## 4. クラス構造の標準

### パフォーマンス規約
```csharp
public class ExampleSystemManager : MonoBehaviour
{
    #region Serialized Fields
    [Header(""🎮 System Configuration"")]
    [SerializeField] private SystemConfig systemConfig;
    [SerializeField] private bool enableFeature = true;
    #endregion

    #region Private Fields
    private Dictionary<string, SystemData> _systemData = new();
    private bool _isInitialized = false;
    #endregion

    #region Public Properties
    public bool IsInitialized => _isInitialized;
    public int SystemCount => _systemData.Count;
    #endregion

    #region Events
    public event Action<SystemData> OnSystemUpdated;
    #endregion

    #region Unity Lifecycle
    private void Start() { }
    private void Update() { }
    private void OnDestroy() { }
    #endregion

    #region Public API
    public void InitializeSystem(SystemConfig config) { }
    public bool ProcessSystemData(string id, SystemData data) { }
    #endregion

    #region Private Methods
    private void ValidateConfiguration() { }
    private SystemData CreateSystemData() { }
    #endregion

    #region Utility Methods
    private static float CalculateValue(float input) { }
    private static bool ValidateInput(string input) { }
    #endregion
}
```

═══════════════════════════════════════════════════════════════════════════════════════

## 5. パフォーマンス規約

### ECS 連携要件
```csharp
// ✅ CORRECT: ECS 互換データ構造
[Serializable]
public struct MonsterPerformance : IComponentData
{
    public float basePerformance;
    public float geneticBonus;
    public float equipmentBonus;
    public float experienceBonus;
}

// ✅ CORRECT: Burst-compatible methods

public static float CalculatePerformance(MonsterPerformance performance)
{
    return performance.basePerformance + performance.geneticBonus +
           performance.equipmentBonus + performance.experienceBonus;
}
```

### メモリ管理
```csharp
// ✅ CORRECT: オブジェクトプールで頻繁生成を最適化する
private ObjectPool<Monster> _monsterPool;
private Dictionary<string, Monster> _activeMonsters = new();

// ✅ CORRECT: Disposeパターンで確実に解放
public void Dispose()
{
    _activeMonsters?.Clear();
    _monsterPool?.Dispose();
    OnSystemUpdated = null;
}

// ✅ CORRECT: よくアクセスするデータはキャッシュ
private readonly Dictionary<string, float> _performanceCache = new();
```

### パフォーマンス目標
```csharp
// - モンスター1000体以上で60FPS維持
// - アクティビティ処理は16ms以内
// - プレイ中のメモリ割り当ては1MB以内
// - ワールド初期化のロード時間は 10 秒以内
```

═══════════════════════════════════════════════════════════════════════════════════════

## 🎨 6. Unity 固有の規約

### Inspector 設定
```csharp
[Header(""🎮 Core Configuration"")]
[SerializeField] private GameConfig gameConfig;
[SerializeField] private bool enableDebugMode = false;

[Header(""⚡ Performance Settings"")]
[SerializeField] [Range(1, 1000)] private int maxCreatures = 100;
[SerializeField] private float updateFrequency = 0.1f;

[Header(""📊 Runtime Status"")]
[SerializeField, ReadOnly] private int activeCreatures = 0;
[SerializeField, ReadOnly] private float lastUpdateTime = 0f;
```

### ScriptableObject 設定
```csharp
[CreateAssetMenu(fileName = ""Equipment Database"", menuName = ""Chimera/Equipment Database"", order = 10)]
public class EquipmentDatabase : ScriptableObject
{
    [Header(""🎒 Equipment Collections"")]
    [SerializeField] private EquipmentConfig[] weapons = new EquipmentConfig[0];
    [SerializeField] private EquipmentConfig[] armor = new EquipmentConfig[0];
}
```

### コンテキストメニュー統合
```csharp
[ContextMenu(""Initialize System"")]
public void InitializeSystem() { }

[ContextMenu(""Run Integration Test"")]
public void RunIntegrationTest() { }

[ContextMenu(""Reset to Defaults"")]
public void ResetToDefaults() { }
```

═══════════════════════════════════════════════════════════════════════════════════════

## 7. エラーハンドリング規約

### 例外処理
```csharp
// ✅ CORRECT: 具体的な例外処理 + フォールバック処理
public bool ProcessMonsterData(Monster monster)
{
    try
    {
        ValidateMonster(monster);
        return ProcessValidMonster(monster);
    }
    catch (ArgumentNullException ex)
    {
        Debug.LogError($""Monster data is null: {ex.Message}"");
        return false;
    }
    catch (InvalidOperationException ex)
    {
        Debug.LogWarning($""Invalid monster state: {ex.Message}"");
        return TryRecoverMonsterState(monster);
    }
    catch (Exception ex)
    {
        Debug.LogError($""Unexpected error processing monster: {ex}"");
        return false;
    }
}
```

### ログ運用
```csharp
// ✅ CORRECT: 一貫性のあるログメッセージ（コンテキスト + 絵文字つき）
Debug.Log(""🧬 Genetic system initialized successfully"");
Debug.LogWarning(""⚠️ Monster happiness below optimal threshold"");
Debug.LogError(""❌ Critical failure in breeding system"");

// ✅ CORRECT: デバッグログの条件付き出力
if (enableDebugLogging)
{
    Debug.Log($""🔬 Breeding result: {parent1.Name} × {parent2.Name} → {offspring.Name}"");
}
```

═══════════════════════════════════════════════════════════════════════════════════════

## 8. テスト規約

### ユニットテスト命名
```csharp
[Test]
public void EquipItem_WithValidMonsterAndEquipment_ShouldReturnTrue()
{
    // 準備
    var monster = CreateTestMonster();
    var equipment = CreateTestEquipment();

    // 実行
    var result = equipmentManager.EquipItem(monster, equipment.ItemId);

    // 検証
    Assert.IsTrue(result);
    Assert.Contains(equipment, monster.Equipment);
}
```

### 統合テストの構造
```csharp
private async UniTask RunTest(string testName, Func<UniTask<bool>> testAction)
{
    try
    {
        var result = await testAction();
        LogTest(result ? $""✅ {testName}"" : $""❌ {testName} - FAILED"");
        return result;
    }
    catch (Exception ex)
    {
        LogTest($""❌ {testName} - EXCEPTION: {ex.Message}"");
        return false;
    }
}
```

═══════════════════════════════════════════════════════════════════════════════════════

## 9. データ構造規約

### シリアライズ可能なデータクラス
```csharp
[Serializable]
public class PlayerProfile
{
    [Header(""Basic Info"")]
    public string PlayerId;
    public string PlayerName;
    public DateTime JoinedDate;

    [Header(""Statistics"")]
    public int SocialRating;
    public int TournamentWins;
    public List<string> Achievements = new();
}
```

### ScriptableObject 設定
```csharp
[CreateAssetMenu(fileName = ""Monster Town Config"", menuName = ""Chimera/Monster Town Config"")]
public class MonsterTownConfig : ScriptableObject
{
    [Header(""🏘️ Town Settings"")]
    public string townName = ""New Monster Town"";
    public int maxPopulation = 100;

    [Header(""💰 Starting Resources"")]
    public TownResources startingResources = TownResources.GetDefault();
}