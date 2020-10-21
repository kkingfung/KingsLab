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
    const int NumofStage = 4;
    public List<UIRecordList> AllRecords;

    List<SaveObject> stageRecords;

    private void OnEnable()
    {
        stageRecords = new List<SaveObject>();
        stageRecords.Clear();

        for (int i = 0; i < NumofStage; i++)
        {
            stageRecords.Add(SaveSystem.LoadObject<SaveObject>("Record" + i.ToString()));
        }
        if (AllRecords.Count > 0)
            updateUI();
    }

    private void OnDisable()
    {
        for (int i = 0; i < NumofStage; i++)
        {
            SaveSystem.SaveObject("Record" + i.ToString(), stageRecords[i],true);
        }
        stageRecords.Clear();
    }


    void RecordComparison(int stageID, string name, int score) {
        stageRecords[stageID]=stageRecords[stageID].InsertObject(stageID, name, score);
        updateUI();
    }

    void updateUI() {
        for (int i = 0; i < NumofStage; i++)
        {
            AllRecords[i].Records[0].text = "1." + stageRecords[i].record1.name.Substring(0, 5).ToUpper() + "   " + stageRecords[i].record1.score.ToString("000000");
            AllRecords[i].Records[1].text = "2." + stageRecords[i].record2.name.Substring(0, 5).ToUpper() + "   " + stageRecords[i].record2.score.ToString("000000");
            AllRecords[i].Records[2].text = "3." + stageRecords[i].record3.name.Substring(0, 5).ToUpper() + "   " + stageRecords[i].record3.score.ToString("000000");
            AllRecords[i].Records[3].text = "4." + stageRecords[i].record4.name.Substring(0, 5).ToUpper() + "   " + stageRecords[i].record4.score.ToString("000000");
            AllRecords[i].Records[4].text = "5." + stageRecords[i].record5.name.Substring(0, 5).ToUpper() + "   " + stageRecords[i].record5.score.ToString("000000");
        }
    }
}

//testing save system
//SaveSystem.Init();

//SaveObject defaultRecord = new SaveObject();

//for (int i = 0; i < 4; i++) {
//    defaultRecord.stageID = i;
//    defaultRecord.record1 = new Record("AAAAA", 50000);
//    defaultRecord.record2 = new Record("BBBBB", 10000);
//    defaultRecord.record3 = new Record("CCCCC", 5000);
//    defaultRecord.record4 = new Record("DDDDD", 1000);
//    defaultRecord.record5 = new Record("EEEEE", 100);
//    SaveSystem.SaveObject("Record" + i.ToString(), defaultRecord, true) ;
//}