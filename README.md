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

**Note:** 本作は、DOTS、ML-Agents、手続き生成、パフォーマンス最適化など、Unity の高度な技術を学ぶ学生プロジェクトです。
