using UnityEngine;

[System.Serializable]
public class BiomeClass
{
    public string biomeName;
    public Color biomeCol;
    public TileAtlas tileAtlas;

    [Header("Noise")]
    public float terrainFreq = 0.05f;
    public float caveFreq = 0.05f;
    public Texture2D caveNoiseTexture;

    [Header("Generation")]
    public bool generateCaves = true;
    public int dirtLayerHeight = 5;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 25f;

    [Header("Cherry on the top")]
    public int treeChance = 15;
    public int mintreeHeight = 3;
    public int maxtreeHeight = 6;

    public int tallGrassChance = 3;

    [Header("Ore")]
    public OreClass[] ores;
}
