using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Data/GameData")]
public class GameData : ScriptableObject
{
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public int lastUnlockedLevel = 1;

    public void Reset()
    {
        musicVolume = 1f;
        sfxVolume = 1f;
        lastUnlockedLevel = 1;
    }
}