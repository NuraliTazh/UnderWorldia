using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TileAtlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Envitonment")]
    public TileClass dirt;
    public TileClass grass;
    public TileClass stone;
    public TileClass log;
    public TileClass leaf;
    public TileClass tallGrass;

    [Header("Ores")]
    public TileClass silver;
    public TileClass coal;
    public TileClass iron;
    public TileClass gold;
    public TileClass diamond;
}
