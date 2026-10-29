using System.IO;
using UnityEngine;

public class DataSaveService
{
    private readonly string _path;

    public DataSaveService(string path)
    {
        _path = path;
    }

    public void Save(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data);

        File.WriteAllText(_path, json);
    }

    public GameSaveData Load()
    {
        if (!File.Exists(_path))
        {
            return null;
        }

        string json = File.ReadAllText(_path);

        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
        return saveData;
    }
}
