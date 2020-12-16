using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIRecordList {
    public List<TextMesh> Records;

}
public class RecordManager : MonoBehaviour
{
    public List<UIRecordList> AllRecords;

    List<SaveObject> stageRecords;
    public InGameOperation sceneManager;
    public int rank;
    private void OnEnable()
    {
        stageRecords = new List<SaveObject>();
        stageRecords.Clear();

        for (int i = 0; i < StageInfo.IslandNum; ++i)
        {
            SaveSystem.Init("Record" + i.ToString());
            stageRecords.Add(SaveSystem.LoadObject<SaveObject>("Record" + i.ToString()));
        }
        if (AllRecords.Count > 0)
            updateUI();
    }

    private void OnDisable()
    {
        for (int i = 0; i < StageInfo.IslandNum; ++i)
        {
            SaveSystem.SaveObject("Record" + i.ToString(), stageRecords[i], true);
        }
        stageRecords.Clear();
    }

    private void Start()
    {
        //sceneManager = FindObjectOfType<InGameOperation>();
    }
    public int RecordComparison(int stageID, string name, int score)
    {
        rank = stageRecords[stageID].InsertObject(stageID, name, score);
 
        if (AllRecords.Count > 0)
            updateUI();

        return rank;
    }

    void updateUI()
    {
        for (int i = 0; i < StageInfo.IslandNum; ++i)
        {
            AllRecords[i].Records[0].text = "1." + stageRecords[i].record1.name.Substring(0, 5).ToUpper() + "\t\t" + stageRecords[i].record1.score.ToString("000000");
            AllRecords[i].Records[1].text = "2." + stageRecords[i].record2.name.Substring(0, 5).ToUpper() + "\t\t" + stageRecords[i].record2.score.ToString("000000");
            AllRecords[i].Records[2].text = "3." + stageRecords[i].record3.name.Substring(0, 5).ToUpper() + "\t\t" + stageRecords[i].record3.score.ToString("000000");
            AllRecords[i].Records[3].text = "4." + stageRecords[i].record4.name.Substring(0, 5).ToUpper() + "\t\t" + stageRecords[i].record4.score.ToString("000000");
            AllRecords[i].Records[4].text = "5." + stageRecords[i].record5.name.Substring(0, 5).ToUpper() + "\t\t" + stageRecords[i].record5.score.ToString("000000");
        }
    }

    public void UpdateRecordName(int rank, string name)
    {
        int currIsland = sceneManager.GetCurrIsland();
        switch (rank)
        {
            case 1: stageRecords[currIsland].record1.name = name; break;
            case 2: stageRecords[currIsland].record2.name = name; break;
            case 3: stageRecords[currIsland].record3.name = name; break;
            case 4: stageRecords[currIsland].record4.name = name; break;
            case 5: stageRecords[currIsland].record5.name = name; break;
            default:return;
        }
        SaveSystem.SaveObject("Record" + currIsland.ToString(), stageRecords[currIsland], true);
    }
}