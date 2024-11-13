using System.Collections.Generic;
using UnityEngine;

// This class is used to store gamestate data that is needed to restore the game after exiting.
// We keep a copy of the data in memory. When the game exits or at designated times, we save the data
// to disk. In the future, we could save the data to the cloud as well to prevent tampering.
public class PersistentData : MonoBehaviour
{
    public static PersistentData Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void StoreString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public string GetString(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            return "null";
        }
        return PlayerPrefs.GetString(key);
    }

    public void StoreStringList(string key, List<string> value)
    {
        // throw an error if any of the strings contain a comma
        foreach (string s in value)
        {
            if (s.Contains(","))
            {
                throw new System.Exception("String contains a comma");
            }
        }
        // combine the list into a comma-separated string
        string combined = string.Join(",", value);
        PlayerPrefs.SetString(key, combined);
    }

    public List<string> GetStringList(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            return new List<string>();
        }
        // split the comma-separated string into a list
        string combined = PlayerPrefs.GetString(key);
        return new List<string>(combined.Split(','));
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }
}
