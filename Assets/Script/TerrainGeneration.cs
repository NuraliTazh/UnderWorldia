using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;


public class TerrainGeneration : MonoBehaviour
{
    [Header("Lighting")]
    public Texture2D worldTilesMap;
    public Material lightShader;
    public float groundLightThreashold = 0.7f;
    public float airLightThreashold = 0.85f;
    public float lightRadius = 7f;

    List<Vector2Int> unlitBlocks = new List<Vector2Int>();

    public PlayerController player;
    public CamController cam;
    public GameObject tileDrop;

    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    public float seed;
    public BiomeClass[] biomes;

    [Header("Biomes")]
    public float biomeFreq;
    public Gradient biomeGradient;
    public Texture2D biomeMap;

    [Header("Generation")]
    public int chunkSize = 20;
    public int worldSize = 100;
    public int heightAddition = 25;
    public bool generateCaves = true;

    [Header("Noise")]
    public Texture2D caveNoiseTexture;
    public float terrainFreq = 0.05f;
    public float caveFreq = 0.05f;

    [Header("Ore")]
    public OreClass[] ores;

    private GameObject[] worldChunks;

    private GameObject[,] world_ForegroundObjects;
    private GameObject[,] world_BackgroundObjects;
    private TileClass[,] world_BackgroundTiles;
    private TileClass[,] world_ForegroundTiles;

    private BiomeClass curBiome;
    private Color[] biomeCols;

    private void Start()
    {
        world_ForegroundTiles = new TileClass[worldSize, worldSize];
        world_BackgroundTiles = new TileClass[worldSize, worldSize];
        world_ForegroundObjects = new GameObject[worldSize, worldSize];
        world_BackgroundObjects = new GameObject[worldSize, worldSize];
        //initilise light
        worldTilesMap = new Texture2D(worldSize, worldSize);
        worldTilesMap.filterMode = FilterMode.Point; //Do light more like pixel
        lightShader.SetTexture("_ShadowTex", worldTilesMap);

        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                worldTilesMap.SetPixel(x, y, Color.white);
            }
        }
        worldTilesMap.Apply();

        //generate Terrain stuff
        seed = Random.Range(-10000, 10000);

        for (int i = 0; i < ores.Length; i++)
        {
            ores[i].spreadTexture = new Texture2D(worldSize, worldSize);
        }

        biomeCols = new Color[biomes.Length];

        for (int i = 0; i < biomes.Length; i++)
        {
            biomeCols[i] = biomes[i].biomeCol;
        }


        DrawBiomeMap();
        DrawCavesAndOres();

        CreateChunks();
        GenerateTerrain();

        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                if (worldTilesMap.GetPixel(x, y) == Color.white)
                    LightBlock(x, y, 1f, 0);
            }
        }

        worldTilesMap.Apply();

        cam.Spawn(new Vector3(player.spawnPos.x, player.spawnPos.y, cam.transform.position.z));
        cam.worldSize = worldSize;
        player.Spawn();
    }
    
    private void Update()
    {
        RefreshChunks();
    }

    void RefreshChunks()
    {
        for (int i = 0; i < worldChunks.Length; i++)
        {
            if (Vector2.Distance(new Vector2((i * chunkSize) + (chunkSize / 2), 0), new Vector2(player.transform.position.x, 0)) > Camera.main.orthographicSize * 5f)
                worldChunks[i].SetActive(false);
            else
                worldChunks[i].SetActive(true);
        }
    }

    public void DrawBiomeMap()
    {
        float b;
        Color col;
        biomeMap = new Texture2D(worldSize, worldSize);
        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                b = Mathf.PerlinNoise((x + seed) * biomeFreq, (y + seed) * biomeFreq);
                col = biomeGradient.Evaluate(b);
                biomeMap.SetPixel(x, y, col);

            }
        }

        biomeMap.Apply();
    }

    public void DrawCavesAndOres()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        float v;
        float o;

        for (int x = 0; x < caveNoiseTexture.width; x++)
        {
            for (int y = 0; y < caveNoiseTexture.height; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                if (v > curBiome.surfaceValue)
                    caveNoiseTexture.SetPixel(x, y, Color.white);
                else
                    caveNoiseTexture.SetPixel(x, y, Color.black);

                for (int i = 0; i < ores.Length; i++)
                {
                    ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (curBiome.ores.Length >= i + 1)
                    {
                        o = Mathf.PerlinNoise((x + seed) * curBiome.ores[i].frequency, (y + seed) * curBiome.ores[i].frequency);
                        if (o > curBiome.ores[i].size)
                            ores[i].spreadTexture.SetPixel(x, y, Color.red);
                    }
                }
            }
        }

        caveNoiseTexture.Apply();

        for (int i = 0; i < ores.Length; i++)
            ores[i].spreadTexture.Apply();
    }   

    public void DrawTexture()
    {
        for (int i = 0; i < biomes.Length; i++)
        {
            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spreadTexture = new Texture2D(worldSize, worldSize);
                GenerateNoiseTexture(biomes[i].ores[o].frequency, biomes[i].ores[o].size, biomes[i].ores[o].spreadTexture);
            }
        }
    }

    public void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
        float v;

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);

                if (v > limit)
                    noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
            }
        }

        noiseTexture.Apply();
    }

    public void CreateChunks()
    {
        int numChunks = Mathf.CeilToInt((float)worldSize / chunkSize);
        worldChunks = new GameObject[numChunks];
        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }

    public BiomeClass GetCurrentBiome(int x, int y)
    {
        Color pixel = biomeMap.GetPixel(x, y);
        int index = System.Array.IndexOf(biomeCols, pixel);
        if (index >= 0)
            return biomes[index];

        return curBiome;
    }

    public void GenerateTerrain()
    {
        TileClass tileClass;
        for (int x = 0; x < worldSize; x++)
        {
            float height;

            for (int y = 0; y < worldSize; y++)
            {
                curBiome = GetCurrentBiome(x, y); //Определение текущего биома
                height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * curBiome.heightMultiplier + heightAddition;
                if (x == worldSize / 2)
                    player.spawnPos = new Vector2(x, height + 2);


                if (y >= height)
                    break;
                if (y < height - curBiome.dirtLayerHeight)
                {
                    tileClass = curBiome.tileAtlas.stone;

                    if (ores[0].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[0].maxSpawnHeight)
                        tileClass = tileAtlas.coal;
                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawnHeight)
                        tileClass = tileAtlas.iron;
                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawnHeight)    
                        tileClass = tileAtlas.gold;
                    if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight)
                        tileClass = tileAtlas.diamond;
                }
                else if (y < height - 1)
                {
                    tileClass = curBiome.tileAtlas.dirt ;
                }
                else
                {
                    tileClass = curBiome.tileAtlas.grass;
                }

                if (generateCaves)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileClass, x, y, true);
                    }
                    else if (tileClass.wallVariant != null)
                    {
                        PlaceTile(tileClass.wallVariant, x, y, true);
                    }
                }
                else
                {
                    PlaceTile(tileClass, x, y, true);
                }

                if (y >= height - 1)
                {
                    int t = Random.Range(0, curBiome.treeChance);

                    if (t == 1)
                    {
                        if (GetTileFromWorld(x, y))
                        {
                            if (curBiome.biomeName == "Dessert")
                            {
                                GenerateCactus(curBiome.tileAtlas, Random.Range(curBiome.mintreeHeight, curBiome.maxtreeHeight), x, y + 1); 
                            }
                            else
                            {
                                GenerateTree(Random.Range(curBiome.mintreeHeight, curBiome.maxtreeHeight), x, y + 1);
                            }

                        }
                    }
                    else
                    {
                        int i = Random.Range(0, curBiome.tallGrassChance);  
                        if (i == 1)
                        {
                            if (GetTileFromWorld(x, y))
                            {
                                if (curBiome.tileAtlas.tallGrass != null)
                                {
                                    PlaceTile(curBiome.tileAtlas.tallGrass, x, y + 1, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        worldTilesMap.Apply();
    }

    public void GenerateCactus(TileAtlas atlas, int treeHeight, int x, int y)
    {
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(atlas.log, x, y + i, true);
        }
    }

    public void GenerateTree(int treeHeight, int x, int y)
    {
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(tileAtlas.log, x, y + i, true);
        }

        PlaceTile(tileAtlas.leaf, x, y + treeHeight, true);
        PlaceTile(tileAtlas.leaf, x, y + treeHeight + 1, true);
        PlaceTile(tileAtlas.leaf, x, y + treeHeight + 2, true);

        PlaceTile(tileAtlas.leaf, x + 1, y + treeHeight, true);
        PlaceTile(tileAtlas.leaf, x + 1, y + treeHeight + 1, true);

        PlaceTile(tileAtlas.leaf, x - 1, y + treeHeight, true);
        PlaceTile(tileAtlas.leaf, x - 1, y + treeHeight + 1, true);
    }

    public void RemoveTile(int x, int y)
    {
        if (GetTileFromWorld(x, y) && x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            TileClass tile = GetTileFromWorld(x, y);
            RemoveTileFromWorld(x, y);
            
            if (tile.wallVariant != null)
            {
                //If the tile naturally placed
                if (tile.naturallyPlaced)
                {
                    PlaceTile(tile.wallVariant, x, y, true);
                }
            }


            //drop tile
            if (tile.tileDrop)
            {
                GameObject newtileDrop = Instantiate(tileDrop, new Vector2(x, y + 0.5f), Quaternion.identity);
                newtileDrop.GetComponent<SpriteRenderer>().sprite = tile.tileDrop;
            }


            if (!GetTileFromWorld(x, y))
            {
                    worldTilesMap.SetPixel(x, y, Color.white);
                    LightBlock(x, y, 1f, 0);
                    worldTilesMap.Apply();
            }

            Destroy(GetObjectFromWorld(x, y));
            RemoveObjectFromWorld(x, y);
        }
    }

    public void CheckTile(TileClass tile, int x, int y, bool isNaturallyPlaced)
    {
        if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
        {
            if (tile.inBackground)
            {
                if (!GetTileFromWorld(x, y).inBackground)
                {
                    RemoveLightSourse(x, y);
                    PlaceTile(tile, x, y, isNaturallyPlaced);
                }
            }
            else
            {
                if (GetTileFromWorld(x + 1, y) ||
                    GetTileFromWorld(x - 1, y) ||
                    GetTileFromWorld(x, y + 1) ||
                    GetTileFromWorld(x, y - 1))
                {

                    if (!GetTileFromWorld(x, y))
                    {
                        RemoveLightSourse(x, y);
                        PlaceTile(tile, x, y, isNaturallyPlaced);
                    }
                    else
                    {
                        if (GetTileFromWorld(x, y).inBackground)
                        {
                            RemoveLightSourse(x, y);
                            PlaceTile(tile, x, y, isNaturallyPlaced);
                        }
                    }
                }
            }
        }
    }
    
    public void PlaceTile(TileClass tile, int x, int y, bool isNaturallyPlaced)
    { 
        if (x >= 0 && x < worldSize && y >= 0 && y < worldSize) //Данная проверка предотвращает выход за границы игрового мира
        {
            GameObject newTile = new GameObject(); //конкретный блок игрового мира
            int chunkCoord = x / chunkSize; //чанк используется как родительский объект
            newTile.transform.parent = worldChunks[chunkCoord].transform; //позволяет группировать блоки внутри чанков
            newTile.AddComponent<SpriteRenderer>(); //отображения блока
            int spriteIndex = Random.Range(0, tile.tileSprites.Length); //блок может иметь несколько вариантов спрайтов
            newTile.GetComponent<SpriteRenderer>().sprite = tile.tileSprites[spriteIndex]; //выбранный спрайт устанавливается

            worldTilesMap.SetPixel(x, y, Color.black); //При размещении блока соответствующий пиксель устанавливается в чёрный цвет
            if (tile.inBackground)
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -10;

                if (tile.name.ToLower().Contains("wall"))
                {
                    newTile.GetComponent<SpriteRenderer>().color = new Color(0.6f, 0.6f, 0.6f);
                }
                else
                {
                    worldTilesMap.SetPixel(x, y, Color.white);
                }
            }
            else
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -5;
                newTile.AddComponent<BoxCollider2D>();
                newTile.GetComponent<BoxCollider2D>().size = Vector2.one;
                newTile.tag = "Ground";
            }

            newTile.name = tile.tileSprites[0].name; //объекту присваивается имя
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f); //позиция в игровом мире

            TileClass newTileClass = TileClass.CreateInstance(tile, isNaturallyPlaced); //хранения информации о блоке

            AddObjectToWorld(x, y, newTile, newTileClass); //хранения визуального объекта
            AddTileToWorld(x, y, newTileClass); //хранения логической информации
        }
    }

    void AddTileToWorld(int x, int y, TileClass tile)
    {
        if (tile.inBackground)
        {
            world_BackgroundTiles[x, y] = tile;
        }
        else
        {
            world_ForegroundTiles[x, y] = tile;
        }
    }

    void AddObjectToWorld(int x, int y, GameObject tileObject, TileClass tile)
    {
        if (tile.inBackground)
        {
            world_BackgroundObjects[x, y] = tileObject;
        }
        else
        {
            world_ForegroundObjects[x, y] = tileObject;
        }
    }

    void RemoveTileFromWorld(int x, int y)
    {
        if (world_ForegroundTiles[x, y] != null)
        {
            world_ForegroundTiles[x, y] = null;
        }
        else if (world_BackgroundTiles[x, y] != null)
        {
            world_BackgroundTiles[x, y] = null;
        }
    }

    void RemoveObjectFromWorld(int x, int y)
    {
        if (world_ForegroundObjects[x, y] != null)
        {
            world_ForegroundObjects[x, y] = null;
        }
        else if (world_BackgroundObjects[x, y] != null)
        {
            world_BackgroundObjects[x, y] = null;
        }
    }

    GameObject GetObjectFromWorld(int x, int y)
    {
        if (world_ForegroundObjects[x, y] != null)
        {
            return world_ForegroundObjects[x, y];
        }
        else if (world_BackgroundObjects[x, y] != null)
        {
            return world_BackgroundObjects[x, y];
        }

        return null;
    }

    TileClass GetTileFromWorld(int x, int y)
    {
        if (world_ForegroundTiles[x, y] != null)
        {
            return world_ForegroundTiles[x, y];
        }
        else if (world_BackgroundTiles[x, y] != null)
        {
            return world_BackgroundTiles[x, y];
        }
        return null;
    }

    void LightBlock (int x, int y, float intensity, int iteration)
    {
        if (iteration < lightRadius)
        {
            worldTilesMap.SetPixel(x, y, Color.white * intensity);

            float thresh = groundLightThreashold;

            if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
            {
                if (world_ForegroundTiles[x, y])
                    thresh = groundLightThreashold;
                else
                    thresh = airLightThreashold;
            }

            for (int nx = x - 1 ; nx < x + 2; nx++)
            {
                for (int ny = y - 1 ; ny < y + 2; ny++)
                {
                    if (nx != x || ny != y)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));  
                        float targetIntensity = Mathf.Pow(thresh, dist) * intensity;

                        if (worldTilesMap.GetPixel(nx, ny) != null)
                        {
                            if (worldTilesMap.GetPixel(nx, ny).r < targetIntensity)
                            {
                                LightBlock(nx, ny, targetIntensity, iteration + 1);
                            }
                        }
                    }
                }
            }

            worldTilesMap.Apply();
        }
    }

    void RemoveLightSourse(int x, int y)
    {
        unlitBlocks.Clear();
        UnLightBlock(x, y, x, y);

        List<Vector2Int> toRelight = new List<Vector2Int>();
        foreach (Vector2Int block in unlitBlocks)
        {
            for (int nx = block.x - 1; nx < block.x + 2; nx++)
            {
                for (int ny = block.y - 1; ny < block.y + 2; ny++)
                {
                    if (worldTilesMap.GetPixel(nx, ny) != null)
                    {
                        if (worldTilesMap.GetPixel(nx, ny).r > worldTilesMap.GetPixel(block.x, block.y).r)
                        {
                            if (!toRelight.Contains(new Vector2Int(nx, ny)))
                                toRelight.Add(new Vector2Int(nx, ny));
                        }
                    }
                }
            } 
                
        }

        foreach (Vector2Int source in toRelight)
        {
            LightBlock(source.x, source.y, worldTilesMap.GetPixel(source.x, source.y).r, 0);
        }

        worldTilesMap.Apply();
    }

    void UnLightBlock(int x, int y, int ix, int iy)
    {
        if (Mathf.Abs(x - ix) >= lightRadius || Mathf.Abs(y - iy) >= lightRadius || unlitBlocks.Contains(new Vector2Int(x, y)))
            return;

        for (int nx = x - 1; nx < x + 2; nx++)
        {
            for (int ny = y - 1; ny < y + 2; ny++)
            {
                if (nx != x || ny != y)
                {
                    if (worldTilesMap.GetPixel(nx, ny) != null)
                    {
                        if (worldTilesMap.GetPixel(nx, ny).r < worldTilesMap.GetPixel(x, y).r)
                        {
                            UnLightBlock(nx, ny, ix, iy);
                        }
                    }
                }
            }
        }

        worldTilesMap.SetPixel(x, y, Color.black);
        unlitBlocks.Add(new Vector2Int(x, y));
    }
}
