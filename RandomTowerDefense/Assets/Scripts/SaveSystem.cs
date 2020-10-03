using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public static class SaveSystem {

    private static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    private const string SAVE_EXTENSION = "txt";

    public static void Init() {
        // Test if Save Folder exists
        if (!Directory.Exists(SAVE_FOLDER)) {
            // Create Save Folder
            Directory.CreateDirectory(SAVE_FOLDER);
        }

        //SaveObject defaultRecord = new SaveObject();
        //Record record;
        //record.name = "AAAAA"; record.score = 50000;
        //defaultRecord.record.Add(record);
        //record.name = "BBBBB"; record.score = 10000;
        //defaultRecord.record.Add(record);
        //record.name = "CCCCC"; record.score = 5000;
        //defaultRecord.record.Add(record);
        //record.name = "DDDDD"; record.score = 1000;
        //defaultRecord.record.Add(record);
        //record.name = "EEEEE"; record.score = 100;
        //defaultRecord.record.Add(record);

        //for (int i = 0; i < 4; i++)
        //{
        //    defaultRecord.stageID = i;
        //    SaveObject("Record", defaultRecord, true);
        //}
    }

    public static void Save(string fileName, string saveString, bool overwrite)
    {
        Init();
        
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
        Init();
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
        Init();
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

    public static void SaveObject(object saveObject)
    {
        SaveObject("save", saveObject, false);
    }

    public static void SaveObject(string fileName, object saveObject, bool overwrite)
    {
        Init();
        string json = JsonUtility.ToJson(saveObject);
     
        Save(fileName, json, overwrite);
    }

    public static TSaveObject LoadMostRecentObject<TSaveObject>()
    {
        Init();
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
        Init();
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

public struct Record
{
    public string name;
    public int score;
}

[Serializable]
public class SaveObject
{
    public int stageID;
    public List<Record> record;

    public SaveObject() {
        record = new List<Record>();
    }
    public void InsertObject(SaveObject savedObj, string newName, int newScore)
    {
        Record newRecord;
        newRecord.name = newName;
        newRecord.score = newScore;

        record.Add(newRecord);
        record = record.OrderByDescending(x => x.score).ToList();

        record.RemoveAt(5);
    }
}

