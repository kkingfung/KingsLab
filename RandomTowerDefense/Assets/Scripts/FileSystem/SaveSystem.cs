using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public static class SaveSystem {

    //private static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    private static readonly string SAVE_FOLDER = Application.persistentDataPath + "/RandomDefender/";
    private const string SAVE_EXTENSION = "txt";

    public static void Init(string fileName) {
        // Test if Save Folder exists
        if (!Directory.Exists(SAVE_FOLDER)) {
            // Create Save Folder
            Directory.CreateDirectory(SAVE_FOLDER);
        }

        if (File.Exists(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION) == false)
        {
            SaveObject defaultRecord = new SaveObject();

            for (int i = 0; i < 4; ++i)
            {
                defaultRecord.stageID = i;
                defaultRecord.record1 = new Record("AAAAA", 5000);
                defaultRecord.record2 = new Record("BBBBB", 1000);
                defaultRecord.record3 = new Record("CCCCC", 500);
                defaultRecord.record4 = new Record("DDDDD", 300);
                defaultRecord.record5 = new Record("EEEEE", 100);
                SaveObject("Record" + i.ToString(), defaultRecord, true);
            }
        }
    }

    public static void Save(string fileName, string saveString, bool overwrite=true)
    {
        string saveFileName = fileName;

        if (!overwrite)
        {
            // Make sure the Save Number is unique so it doesnt overwrite a previous save file
            int saveNumber = 1;
            while (File.Exists(SAVE_FOLDER + saveFileName + "." + SAVE_EXTENSION))
            {
                saveNumber++;
                saveFileName = fileName + "_" + saveNumber;
            }
            // saveFileName is unique
        }
        File.WriteAllText(SAVE_FOLDER + saveFileName + "." + SAVE_EXTENSION, saveString);
    }

    public static string Load(string fileName)
    {
        if (File.Exists(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION))
        {
            string saveString = File.ReadAllText(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION);
            return saveString;
        }
        else
        {
            return null;
        }
    }

    public static string LoadMostRecentFile()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        // Get all save files
        FileInfo[] saveFiles = directoryInfo.GetFiles("*." + SAVE_EXTENSION);
        // Cycle through all save files and identify the most recent one
        FileInfo mostRecentFile = null;
        foreach (FileInfo fileInfo in saveFiles)
        {
            if (mostRecentFile == null)
            {
                mostRecentFile = fileInfo;
            }
            else
            {
                if (fileInfo.LastWriteTime > mostRecentFile.LastWriteTime)
                {
                    mostRecentFile = fileInfo;
                }
            }
        }

        // If theres a save file, load it, if not return null
        if (mostRecentFile != null)
        {
            string saveString = File.ReadAllText(mostRecentFile.FullName);
            return saveString;
        }
        else
        {
            return null;
        }
    }

    public static void SaveObject(SaveObject saveObject)
    {
        SaveObject("save", saveObject, false);
    }

    public static void SaveObject(string fileName, SaveObject saveObject, bool overwrite)
    {
        string json = JsonUtility.ToJson(saveObject);
        //Debug.Log(json);
        Save(fileName, json, overwrite);
    }

    public static TSaveObject LoadMostRecentObject<TSaveObject>()
    {
        string saveString = LoadMostRecentFile();
        if (saveString != null)
        {
            TSaveObject saveObject = JsonUtility.FromJson<TSaveObject>(saveString);
            return saveObject;
        }
        else
        {
            return default(TSaveObject);
        }
    }

    public static TSaveObject LoadObject<TSaveObject>(string fileName)
    {
        string saveString = Load(fileName);
        if (saveString != null)
        {
            TSaveObject saveObject = JsonUtility.FromJson<TSaveObject>(saveString);
            return saveObject;
        }
        else
        {
            return default(TSaveObject);
        }
    }
}

[Serializable]
public struct Record
{
    public string name;
    public int score;
    public Record(string name, int score) {
        this.name = name;
        this.score = score;
    }
}

public class SaveObject
{
    public int stageID;
    public Record record1;
    public Record record2;
    public Record record3;
    public Record record4;
    public Record record5;

    public int InsertObject(int stageID, string name, int score)
    {
        var recordList = new Record[] { record1, record2, record3, record4, record5 };
        int rank;
        for (rank = recordList.Length; rank > 0; --rank)
        {
            if (recordList[rank-1].score >= score)
            {
                break;
            }
        }
        
        PlayerPrefs.SetInt("PlayerRank", rank + 1);

        if (rank >= recordList.Length) return rank + 1;
        
        for (int sortID = recordList.Length-2; sortID >=rank; --sortID)
        {
            recordList[sortID + 1] = new Record(recordList[sortID].name, recordList[sortID].score);
        }
        recordList[rank].name = name;
        recordList[rank].score = score;

        record1.name = recordList[0].name;
        record1.score = recordList[0].score;
        record2.name = recordList[1].name;
        record2.score = recordList[1].score;
        record3.name = recordList[2].name;
        record3.score = recordList[2].score;
        record4.name = recordList[3].name;
        record4.score = recordList[3].score;
        record5.name = recordList[4].name;
        record5.score = recordList[4].score;

        return rank + 1;

        ////Alternative
        //if (stageID != this.stageID) return null;
        //List<Record> recordHolder = new List<Record>();
        //recordHolder.Add(record1);
        //recordHolder.Add(record2);
        //recordHolder.Add(record3);
        //recordHolder.Add(record4);
        //recordHolder.Add(record5);
        //
        //Record newRecord= new Record(name, score);
        //recordHolder.Add(newRecord);
        //
        //recordHolder = recordHolder.OrderByDescending(x => x.score).ToList();
        //
        //for (int i = recordHolder.Count; i>0;--i) {
        //    if (recordHolder[i-1].name == name && recordHolder[i-1].score == score) {
        //        PlayerPrefs.SetInt("PlayerRank",i);
        //        break;
        //    }
        //}
        //
        //record1 = recordHolder[0];
        //record2 = recordHolder[1];
        //record3 = recordHolder[2];
        //record4 = recordHolder[3];
        //record5 = recordHolder[4];

        //recordHolder.Clear();
    }
}

