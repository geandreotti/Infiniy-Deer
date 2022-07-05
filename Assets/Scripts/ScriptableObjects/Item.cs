using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Item")]
public class Item : ScriptableObject
{
    public string name;
    public GameObject prefab;
    public Texture2D icon;
}
