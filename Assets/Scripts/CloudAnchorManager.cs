using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class CloudAnchorManager
{
    private static readonly string PrefKeyIntAnchorCount = "PrefKeyIntAnchorCount";
    private static readonly string PrefKeyStringAnchor = "PrefKeyStringAnchor_{0}";

    private static Dictionary<string, string> sPrefKeyDic = new Dictionary<string, string>();
    private static Dictionary<string, string> sAnchorIdKeyDic = new Dictionary<string, string>();

    public static int Count
    {
        get
        {
            RestoreAll();
            return sPrefKeyDic.Count;
        }
    }

    public static bool GetCloudAnchorId(int index, out string value)
    {
        bool ret = true;
        string key = string.Format(PrefKeyStringAnchor, index);

        lock (sPrefKeyDic)
        {
            if (!sPrefKeyDic.TryGetValue(key, out value))
            {
                value = PlayerPrefs.GetString(key, null);
                if (value == null)
                {
                    ret = false;
                }
                else
                {
                    sPrefKeyDic[key] = value;
                    sAnchorIdKeyDic[value] = key;
                }
            }
        }

        return ret;
    }

    public static void Append(string value)
    {
        lock (sPrefKeyDic)
        {
            if (!sPrefKeyDic.ContainsValue(value))
            {
                int nextIndex = sPrefKeyDic.Count;
                string key = string.Format(PrefKeyStringAnchor, nextIndex);

                sPrefKeyDic[key] = value;
                sAnchorIdKeyDic[value] = key;
                PlayerPrefs.SetString(key, value);
                PlayerPrefs.Save();
            }
        }
    }

    public static void Remove(string value)
    {
        lock (sPrefKeyDic)
        {
            string key = null;
            if (sAnchorIdKeyDic.TryGetValue(value, out key))
            {
                sAnchorIdKeyDic.Remove(value);
                sPrefKeyDic.Remove(key);

                PlayerPrefs.DeleteKey(key);
            }
        }
    }

    private static void RestoreAll()
    {
        int count = PlayerPrefs.GetInt(PrefKeyIntAnchorCount);

        lock (sPrefKeyDic)
        {
            sPrefKeyDic.Clear();
            for (int i = 0; i < count; i++)
            {
                string key = string.Format(PrefKeyStringAnchor, i);
                string value = sPrefKeyDic[key] = PlayerPrefs.GetString(key);
                sAnchorIdKeyDic[value] = key;
            }
        }
    }

    public static void SaveAll()
    {
        lock (sPrefKeyDic)
        {
            PlayerPrefs.SetInt(PrefKeyIntAnchorCount, sPrefKeyDic.Count);

            foreach (KeyValuePair<string, string> kvPair in sPrefKeyDic)
            {
                PlayerPrefs.SetString(kvPair.Key, kvPair.Value);
            }

            PlayerPrefs.Save();
        }
    }
}