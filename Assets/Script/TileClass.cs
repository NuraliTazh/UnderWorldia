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
    public bool inBackground = false;
    public Sprite tileDrop;
    public bool naturallyPlaced = true;

    public static TileClass CreateInstance(TileClass tile, bool isNaturallyPlaced)
    {
        var thisTile = ScriptableObject.CreateInstance<TileClass>();
        thisTile.Init(tile, isNaturallyPlaced);

        return thisTile;
    }
    public void Init(TileClass tile, bool isNaturallyPlaced)
    {
        tileName = tile.tileName;
        wallVariant = tile.wallVariant;
        tileSprites = tile.tileSprites;
        inBackground = tile.inBackground;
        tileDrop = tile.tileDrop;
        naturallyPlaced = isNaturallyPlaced;
    }
}