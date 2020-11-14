using UnityEngine;
using UnityEngine.Networking; 
using System.Collections;

using System.IO;

public static class LoadBundle
{
    static AssetBundle bundle;

    //IEnumerator GetAssetBundle()
    //{
    //    UnityWebRequest www = new UnityWebRequest("http://www.my-server.com");
    //    DownloadHandlerAssetBundle handler = new DownloadHandlerAssetBundle(www.url, uint.MaxValue);
    //    www.downloadHandler = handler;
    //    yield return www.SendWebRequest();

    //    if (www.isNetworkError || www.isHttpError)
    //    {
    //        Debug.Log(www.error);
    //    }
    //    else
    //    {
    //        // AssetBundle を抽出
    //        bundle = handler.assetBundle;
    //          TextAsset charDataFile = bundle.LoadAsset(filename) as TextAsset;
    //          //string[] linesFromfile = charDataFile.text.Split('\n');

    //          var textFile = File.ReadAllText(charDataFile.text);
    //          File.WriteAllText(filepath+filename, textFile);
    //    }
    //}

    public static void LoadAssetBundle(string bundleUrl, string filename, string filepath)
    {
        bundle = AssetBundle.LoadFromFile(bundleUrl);
        Debug.Log(bundle == null ? "Fail to Load Bundle" : "Bundle loaded");

        TextAsset charDataFile = bundle.LoadAsset(filename) as TextAsset;
        //string[] linesFromfile = charDataFile.text.Split('\n');

        var textFile = File.ReadAllText(charDataFile.text);
        File.WriteAllText(filepath+filename, textFile);
    }
}