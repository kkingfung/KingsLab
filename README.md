# RandomTowerDefense

Unity DOTS と ML-Agents を活用した、AI 駆動型の高度なタワーディフェンスゲームです。KingsLab による学校の最終年度プロジェクトとして制作されました。
現在は職場での経験を活かし、各処理の作り直しを進めています。その後、ゲーム性を維持しつつ操作性と UI の改善を行う予定です。（対応ブランチ: refactor）

## Overview

RandomTowerDefense は、タワーを配置する際に種類がランダムで決まる独自の仕組みにより、ローグライク的な戦略体験を提供します。  
プレイヤーは RNG に適応しつつ、合成システムで強力な上位タワーを作り出す必要があります。

**開発期間:** 3か月  （リメーク含まらない）
**エンジン:** Unity (DOTS/ECS アーキテクチャ)

## Key Features
### Gameplay Mechanics
- **ランダムタワー割り当て:** 各タワー設置時に 4 種類（Nightmare、SoulEater、TerrorBringer、Usurper）の中からランダムに選択
- **タワー合成システム:** 同タイプ・同ランクのタワー 3 つを合成して上位ランクのタワーを作成（ランク 1-4）
- **レベル進行:** タワーは戦闘で経験値を獲得してレベルアップ
- **コレクションボーナス:** 全ランクの同タイプまたは全タイプの同ランクを集めると報酬獲得
- **魔法スキルシステム:** Meteor、Blizzard、Minions、Petrification（効果はアップグレード可能）

### AI Training with ML-Agents
- **デュアルエージェントシステム:** タワービルダー AI 対 敵スポナー AI
- **競合トレーニング:** 両エージェントが互いの戦略に対抗して学習
- **空間認識:** クアドラントベースの観測で戦術判断
- **強化学習:** PPO（Proximal Policy Optimization）を使用したトレーニング

### Procedural Systems
- **マップ生成:** パス検証付きの動的な柱配置
- **逆運動学（IK）:** アニメーションクリップなしでクリーチャーの脚動作を手続き的に生成
- **Boids フロッキング:** GPU 加速による群れ行動シミュレーション
- **動的ウェーブ生成:** 難易度に応じた敵スポーンの適応型生成

## Technical Architecture

### DOTS (Data-Oriented Technology Stack)
本ゲームは、従来の Unity GameObject と DOTS を組み合わせたハイブリッド ECS アーキテクチャを採用。

**DOTS-Powered Systems:**
- 空間パーティショニングを用いたマルチスレッドのタワーターゲティング
- Burst コンパイル済みの衝突判定
- ジョブベースのパスファインディング
- 敵・弾丸のエンティティコンポーネントシステム

**従来の MonoBehaviour:**
- UI とプレイヤー入力
- ビジュアルエフェクトとアニメーション
- シーン管理
- ゲームステートコントローラー

### Performance Optimizations
- **Burst Compiler:** 主要システムのネイティブコード生成
- **Job System:** 戦闘計算のマルチスレッド処理
- **空間パーティショニング:** クアドラントベースで近隣検索を効率化
- **オブジェクトプーリング:** 敵・タワー・弾丸・VFX の再利用
- **NativeCollections:** メモリ効率の高いデータ構造

## Technologies Used

- **Unity 2021+**（DOTS/ECS）
- **Unity ML-Agents**（強化学習）
- **Unity VFX Graph**（GPU 加速パーティクル）
- **Unity Burst Compiler**
- **Unity Job System**
- **Compute Shaders**（Boids 用）
- **A* Pathfinding**（グリッドベースのナビゲーション）

## Project Structure

```
RandomTowerDefense/
├── Assets/
│   ├── Scripts/
│   │   ├── DOTS/            　# ECS 実装
│   │   │   ├── Components/    # ECS データコンポーネント
│   │   │   ├── Systems/       # Job ベースのシステム（ターゲット、衝突、タイマー）
│   │   │   ├── Pathfinding/   # ECS での A* パスファインディング
│   │   │   ├── Spawner/       # エンティティスポーンシステム
│   │   │   └── Tags/          # ECS タグ
│   │   ├── Managers/          # ゲームステート管理
│   │   │   ├── System/        # コアシステム（リソース、ステージ、ウェーブ）
│   │   │   └── Macro/         # 高レベル管理
│   │   ├── Units/             # タワー、敵、城の実装
│   │   ├── Info/              # タワー/敵データ定義
│   │   ├── Boids/             # 群れ行動シミュレーション
│   │   ├── ProcedualAnimation/ # IK 手続きアニメーション
│   │   ├── MapGenerator/      # マップ生成
│   │   ├── Scene/             # シーン管理
│   │   ├── Tools/             # 開発用ツール
│   │   └── Common/            # 共通ユーティリティ
│   ├── Materials/             # マテリアル
│   ├── Prefabs/               # プレハブ
│   ├── Scenes/                # Unity シーン
│   ├── Shaders/               # カスタムシェーダ
│   ├── ML-Agents/             # AI トレーニング設定
│   └── ParticleSystems/       # VFX Graph エフェクト
├── ProjectSettings/           # Unity プロジェクト設定
└── Packages/                  # パッケージ依存
```

### Tower Types
1. **Nightmare (黄/雷):** 長射程、低攻撃速度
2. **SoulEater (緑/毒):** 高速攻撃＋範囲ダメージ
3. **TerrorBringer (青/投射):** 超低速だが高ダメージ
4. **Usurper (赤/火):** 超高速＋範囲ダメージ

### Tower Progression
- **ランク:** 1 → 2 → 3 → 4（同タイプタワー 3 つを合成）
- **ランクごとの最大レベル:** 5、10、20、40
- **経験値システム:** タワーは戦闘でレベルアップ

### Skills System
- **Meteor:** 範囲爆発ダメージ
- **Blizzard:** 広範囲スロー効果
- **Minions:** 一時的な味方ユニットを召喚
- **Petrification:** 敵をその場で停止

## Setup Instructions

### Prerequisites
- Unity 2021.x 以降
- ML-Agents Python パッケージ（AI トレーニング用）
- Git LFS（大容量アセット用）

### Installation
1. リポジトリをクローン:
   ```bash
   git clone https://github.com/kkingfung/KingsLab.git
   ```
2. Unity Hub でプロジェクトを開く
3. Unity がすべてのアセットをインポート（初回は数分かかる場合があります）
4. メインシーンを開く: `Assets/Scenes/[MainScene].unity`

### Building
本番ビルド前に:
1. プロジェクト内の不要なアイテムを非アクティブ化または削除（`RandomTowerDefense/readme.txt` を参照）
2. ターゲットプラットフォーム用にビルド設定を構成
3. すべてのアセットバンドルが正しく設定されていることを確認

### ML-Agents Training (Optional)
1. ML-Agents をインストール: `pip install mlagents`
2. `Assets/ML-Agents/` 内のトレーニングパラメータを設定
3. トレーニングを実行: `mlagents-learn [config].yaml --run-id=[name]`

## Design Patterns

- **Entity Component System (ECS):** パフォーマンス重視のデータ指向設計
- **Manager Pattern:** ゲームシステムの責務分離
- **Object Pooling:** メモリ効率の高いエンティティ再利用
- **Data-Driven Design:** コンフィグベースのゲームバランス
- **State Machine:** ゲームフローとウェーブ進行の管理

## Performance Targets

- 数百のアクティブエンティティで 60+ FPS
- モバイル対応アーキテクチャ（タッチ操作）
- プーリングと ECS による効率的なメモリ管理
- マルチスレッドでの負荷分散

## Notable Achievements

- DOTS と従来型 Unity ワークフローの統合に成功
- 競合型マルチエージェント強化学習
- アニメーションクリップ不要の IK 手続きモーション
- リアルタイムターゲティングのための空間パーティショニング
- 戦闘計算の Burst コンパイル済みジョブシステム
- VFX Graph とゲームプレイシステムの統合

## Credits

**Created by:** KingsLab  
**Project Type:** 学校の最終年度プロジェクト  
**Development Period:** 3か月

## License

本プロジェクトは教育目的として制作されたものです。

---

# RandomTowerDefense

A technically advanced tower defense game with AI-driven gameplay, built with Unity DOTS and ML-Agents. Created by KingsLab as a school final year project.

## Overview

RandomTowerDefense puts a unique twist on the tower defense genre: tower types are randomly assigned when placed, creating a roguelike strategic experience. Players must adapt to RNG while using a merge system to create powerful upgraded towers.

**Development Time:** 3 months
**Engine:** Unity (DOTS/ECS Architecture)

## Key Features

### Gameplay Mechanics
- **Random Tower Assignment:** Each tower placement randomly selects from 4 types (Nightmare, SoulEater, TerrorBringer, Usurper)
- **Tower Merging System:** Combine 3 towers of the same type and rank to create one higher-rank tower (Ranks 1-4)
- **Progressive Leveling:** Towers gain experience and level up through combat
- **Collection Bonuses:** Rewards for collecting all ranks of a type or all types of a rank
- **Magic Skills System:** Meteor, Blizzard, Minions, and Petrification with upgradeable effects

### AI Training with ML-Agents
- **Dual-Agent System:** Tower builder AI vs Enemy spawner AI
- **Competitive Training:** Both agents learn to counter each other's strategies
- **Spatial Reasoning:** Quadrant-based observations for tactical decision-making
- **Reinforcement Learning:** PPO (Proximal Policy Optimization) training

### Procedural Systems
- **Map Generation:** Dynamic pillar placement with pathfinding validation
- **Inverse Kinematics:** Procedural creature leg animation without animation clips
- **Boids Flocking:** GPU-accelerated creature behavior simulation
- **Dynamic Wave Composition:** Adaptive enemy spawning based on difficulty

## Technical Architecture

### DOTS (Data-Oriented Technology Stack)
The game uses a hybrid ECS architecture combining traditional Unity GameObjects with DOTS for performance:

**DOTS-Powered Systems:**
- Multi-threaded tower targeting with spatial partitioning
- Burst-compiled collision detection
- Job-based pathfinding
- Entity component system for enemies and projectiles

**Traditional MonoBehaviour:**
- UI and player input
- Visual effects and animations
- Scene management
- Game state controllers

### Performance Optimizations
- **Burst Compiler:** Native code generation for critical systems
- **Job System:** Multi-threaded processing for combat calculations
- **Spatial Partitioning:** Quadrant-based system for efficient neighbor searches
- **Object Pooling:** Reusable entities for enemies, towers, projectiles, and VFX
- **NativeCollections:** Memory-efficient data structures

## Technologies Used

- **Unity 2021+** with DOTS/ECS
- **Unity ML-Agents** for reinforcement learning
- **Unity VFX Graph** for GPU-accelerated particle effects
- **Unity Burst Compiler** for performance optimization
- **Unity Job System** for multi-threading
- **Compute Shaders** for boids simulation
- **A* Pathfinding** with grid-based navigation

## Project Structure

```
RandomTowerDefense/
├── Assets/
│   ├── Scripts/
│   │   ├── DOTS/              # Entity Component System implementation
│   │   │   ├── Components/    # ECS data components
│   │   │   ├── Systems/       # Job-based systems (targeting, collision, timers)
│   │   │   ├── Pathfinding/   # A* pathfinding with ECS
│   │   │   ├── Spawner/       # Entity spawning systems
│   │   │   └── Tags/          # ECS entity tags
│   │   ├── Managers/          # Game state controllers
│   │   │   ├── System/        # Core systems (resource, stage, wave)
│   │   │   └── Macro/         # High-level game management
│   │   ├── Units/             # Tower, Enemy, Castle implementations
│   │   ├── Info/              # Data definitions (tower types, enemy stats)
│   │   ├── Boids/             # Flocking simulation
│   │   ├── ProcedualAnimation/ # IK system for creature locomotion
│   │   ├── MapGenerator/      # Procedural level generation
│   │   ├── Scene/             # Scene management and transitions
│   │   ├── Tools/             # Development utilities
│   │   └── Common/            # Shared utilities
│   ├── Materials/             # Rendering materials
│   ├── Prefabs/               # Game object prefabs
│   ├── Scenes/                # Unity scenes
│   ├── Shaders/               # Custom shaders
│   ├── ML-Agents/             # AI training configurations
│   └── ParticleSystems/       # VFX Graph effects
├── ProjectSettings/           # Unity project settings
└── Packages/                  # Package dependencies
```

## Game Systems

### Tower Types
1. **Nightmare (Yellow/Lightning):** Long-range, slow attack speed
2. **SoulEater (Green/Poison):** Fast attacking with area damage
3. **TerrorBringer (Cyan/Projectile):** Very slow but high damage
4. **Usurper (Red/Fire):** Very fast with area damage

### Tower Progression
- **Ranks:** 1 → 2 → 3 → 4 (merge 3 same-type towers)
- **Max Levels per Rank:** 5, 10, 20, 40
- **Experience System:** Towers level up through combat

### Skills System
- **Meteor:** Area damage with explosive impact
- **Blizzard:** Slowing effect over large area
- **Minions:** Spawn temporary ally units
- **Petrification:** Freeze enemies in place

## Setup Instructions

### Prerequisites
- Unity 2021.x or later
- ML-Agents Python package (for AI training)
- Git LFS (for large assets)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/kkingfung/KingsLab.git
   ```
2. Open the project in Unity Hub
3. Let Unity import all assets (first import may take several minutes)
4. Open the main scene: `Assets/Scenes/[MainScene].unity`

### Building
Before building for production:
1. Inactive/delete non-necessary items in the project (see `RandomTowerDefense/readme.txt`)
2. Configure build settings for target platform
3. Ensure all asset bundles are properly configured

### ML-Agents Training (Optional)
1. Install ML-Agents: `pip install mlagents`
2. Configure training parameters in `Assets/ML-Agents/`
3. Run training: `mlagents-learn [config].yaml --run-id=[name]`

## Design Patterns

- **Entity Component System (ECS):** Data-oriented design for performance
- **Manager Pattern:** Separation of concerns for game systems
- **Object Pooling:** Memory-efficient entity reuse
- **Data-Driven Design:** Configuration-based game balance
- **State Machine:** Game flow and wave progression control

## Performance Targets

- 60+ FPS with hundreds of active entities
- Mobile-ready architecture with touch controls
- Efficient memory usage through pooling and ECS
- Multi-threaded workload distribution

## Notable Achievements

- Successfully integrated DOTS with traditional Unity workflow
- Competitive multi-agent reinforcement learning
- Procedural inverse kinematics without animation clips
- Efficient spatial partitioning for real-time targeting
- Burst-compiled job systems for combat calculations
- VFX Graph integration with gameplay systems

## Credits

**Created by:** KingsLab
**Project Type:** School Final Year Project
**Development Period:** 3 months

## License

This project is created for educational purposes as part of school coursework.
