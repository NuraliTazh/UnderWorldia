using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "newtileClass", menuName = "Tile Class")]
public class TileClass: ScriptableObject
{
    public string tileName;
    public Sprite[] tileSprites;
    public bool inBackgrownd = true;
    public bool tileDrop = true;

}