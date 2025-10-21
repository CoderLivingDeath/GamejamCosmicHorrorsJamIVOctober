using UnityEngine;

public class PlayerPrefsViewer : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteAll();

        string[] keys = { "Key_Blue", "Key_Red", "Key_Yellow" };
        foreach (string key in keys)
        {
            if (PlayerPrefs.HasKey(key))
                Debug.Log($"{key}: {PlayerPrefs.GetInt(key, 0).ToString()}");
        }
    }
}