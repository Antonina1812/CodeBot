using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string Path =>
        Application.persistentDataPath + "/save.json";

    public static bool HasSave()
    {
        return File.Exists(Path);
    }

    public static void Save(GameData data)
    {
        try
        {
            File.WriteAllText(Path, JsonUtility.ToJson(data));
            Debug.Log($"[SaveSystem] Сохранено: {Path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Ошибка сохранения: {e.Message}");
        }
    }

    public static bool Load(GameData data)
    {
        try
        {
            if (!File.Exists(Path))
                return false;

            JsonUtility.FromJsonOverwrite(File.ReadAllText(Path), data);
            Debug.Log($"[SaveSystem] Загружено: {Path}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] Ошибка загрузки: {e.Message}");
            return false;
        }
    }

    public static void DeleteSave()
    {
        try
        {
            if (!File.Exists(Path)) return;
            File.Delete(Path);
        }
        catch (Exception )
        {

        }
    }
}