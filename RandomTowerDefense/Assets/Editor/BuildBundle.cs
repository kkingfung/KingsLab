using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Windows;

using System.IO;

public class BuildBundle:Editor
{
    [MenuItem("Assets/ BuildAssetBundles")]
    static void Build() 
    {
        // 成果物を出力するフォルダを指定する（プロジェクトフォルダからの相対パス）
        var targetDir = "Assets/AssetBundles";

        if (!UnityEngine.Windows.Directory.Exists(targetDir))
            UnityEngine.Windows.Directory.CreateDirectory(targetDir);

        var builds = new List<AssetBundleBuild>();

        // AssetBundle名とそれに含めるアセットを指定する
        var buildTower = new AssetBundleBuild();
        buildTower.assetBundleName = "TowerInfo";
        buildTower.assetNames = new string[1] { targetDir + "TowerInfo.txt" };

        var buildEnemy = new AssetBundleBuild();
        buildEnemy.assetBundleName = "EnemyInfo";
        buildEnemy.assetNames = new string[1] { targetDir + "EnemyInfo.txt" };

        var buildSkill = new AssetBundleBuild();
        buildSkill.assetBundleName = "SkillInfo";
        buildSkill.assetNames = new string[1] { targetDir + "SkillInfo.txt" };

        var buildEasyStage = new AssetBundleBuild();
        buildEasyStage.assetBundleName = "EasyStageInfo";
        buildEasyStage.assetNames = new string[1] { targetDir + "EasyStageInfo.txt" };

        var buildNormalStage = new AssetBundleBuild();
        buildNormalStage.assetBundleName = "NormalStageInfo";
        buildNormalStage.assetNames = new string[1] { targetDir + "NormalStageInfo.txt" };

        var buildHardStage = new AssetBundleBuild();
        buildHardStage.assetBundleName = "HardStageInfo";
        buildHardStage.assetNames = new string[1] { targetDir + "HardStageInfo.txt" };

        builds.Add(buildTower);
        builds.Add(buildEnemy);
        builds.Add(buildSkill);

        builds.Add(buildEasyStage);
        builds.Add(buildNormalStage);
        builds.Add(buildHardStage);

        // Android用に出力
        var buildTarget = BuildTarget.Android;

        // LZ4で圧縮するようにする
        var buildOptions = BuildAssetBundleOptions.ChunkBasedCompression;

        Debug.Log(BuildPipeline.BuildAssetBundles(targetDir, builds.ToArray(), buildOptions, buildTarget));
    }
}
