using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "newtileClass", menuName = "Tile Class")]
public class TileClass: ScriptableObject
{
    public string tileName;
    public TileClass wallVariant;
    public Sprite[] tileSprites;
    public bool inBackgrownd = false;
    public bool tileDrop = true;
    public bool naturallyPlaced = true;
}