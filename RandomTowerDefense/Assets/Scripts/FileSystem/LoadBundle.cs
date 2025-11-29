using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

using System.IO;

public static class LoadBundle
{
    static AssetBundle bundle;

    public static void LoadAssetBundle(string bundleUrl, string filename, string filepath)
    {
        bundle = AssetBundle.LoadFromFile(bundleUrl);
        //Debug.Log(bundle == null ? "Fail to Load Bundle" : "Bundle loaded");

        TextAsset charDataFile = bundle.LoadAsset(filename) as TextAsset;
        string[] linesFromfile = charDataFile.text.Split('\n');

        var textFile = File.ReadAllText(charDataFile.text);
        File.WriteAllText(filepath + filename, textFile);
    }
}