using System.Collections.Generic;
using UnityEngine;
using Settworks.Hexagons;
using System;
using Unity.Mathematics;
using System.Linq;
using Unity.Burst;
using UnityEngine.UIElements;

public class TileManager : MonoBehaviour {
    [SerializeField] I_Tile[,] gameTiles;
    private static TileManager _inst = null;
    public static TileManager inst
    {
        get
        {
            if (_inst == null)
                _inst = FindObjectOfType(typeof(TileManager)) as TileManager;

            return _inst;
        }
        set
        {
            _inst = value;
        }
    }
    [SerializeField] Transform tileContainer;
    [SerializeField] int tileWidth;
    [SerializeField] int tileHeight;
    
    // This should automatically read in from the file of tiles in an ideal world.
    [SerializeField] List<GameObject> tilePallette;
    // [SerializeField] GameObject mine;
    int qMin = 0;
    int qMax = int.MinValue;
    [SerializeField] int currentMapID = 0;
    [SerializeField] int[,] tileArray;
    [Header("Debug")]
    [SerializeField] bool randomMap = true;
    [SerializeField] Matrix<int> tempMap = new();
    // {
    //     {1,1,1,1,1,2,2,2,3,1,1},
    //     {1,1,1,1,1,3,2,3,1,1,1},
    //     {1,1,1,1,1,3,3,1,1,1,1},
    //     {2,3,3,1,1,1,2,2,2,3,2},
    //     {2,2,3,1,1,1,2,2,3,3,3},
    //     {1,2,2,1,1,1,3,1,3,3,1},
    //     {1,1,2,2,1,1,1,3,1,1,1},
    //     {1,1,2,2,1,1,3,3,1,1,1},
    //     {1,1,1,1,1,1,3,1,1,1,1},
    //     {1,1,1,1,1,3,3,1,1,3,1},
    //     {1,1,1,1,3,3,1,1,3,3,3}
    // };
    public List<GameObject> moveHighlightTiles = new();
    [SerializeField] GameObject moveHighlightPrefab;
    [SerializeField] Transform highlightBucket;
    [SerializeField] LineRenderer edgeLineRenderer;
    public bool debugFlag = false;
    public void HighlightTiles(List<HexCoord> tiles) {
        foreach (GameObject hTile in moveHighlightTiles) {
            Destroy(hTile);
        }
        if(tiles==null) {
            // edgeLineRenderer.positionCount=0;
            return;
        }
        List<Vector2> edges = new();
        List<float> angles = new();
        // List<HexCoord> coveredEdges = new();
        float maxHeight = 0;
        foreach (var tile in tiles) {
            int neighborNumb = 0;
            foreach (HexCoord neighbor in tile.Neighbors()) {
                if(!IsOnTile(neighbor) || !tiles.Contains(neighbor)) {
                    Vector2 tempC1= tile.Corner((neighborNumb-1)%6);
                    float height1 = (tile.q * Mathf.Abs(tile.r) % 4 * 0.1f) + 0.1f;
                    float height2 = (neighbor.q * Mathf.Abs(neighbor.r) % 4 * 0.1f) + 0.1f;
                    height1 = height1 > height2 ? height1 : height2;
                    if(height1>maxHeight) {
                        maxHeight=height1;
                        // edges.Add(new Vector3(edges[^1].x,height1,edges[^1].z));
                    }
                    Vector2 tempC2= tile.Corner(neighborNumb%6);
                    Vector3 center = tempC2+((tempC1-tempC2)/2);
                    angles.Add(Mathf.Atan2(tempC1.x-tempC2.x,tempC1.y-tempC2.y+0.001f)*Mathf.Rad2Deg);
                    edges.Add(center);
                    // edges.Add(new Vector3(tempC.x,height1,tempC.y));
                    // height=height1;
                }
                neighborNumb++;
            }
        }
        // edgeLineRenderer.positionCount=edges.Count;
        // edgeLineRenderer.SetPositions(edges.ToArray());
        for (int i = 0;i < edges.Count;i++) {
            GameObject temp = Instantiate(
                moveHighlightPrefab,
                new (edges[i].x,maxHeight,edges[i].y),
                Quaternion.Euler(0,angles[i],0),
                highlightBucket
            );
            moveHighlightTiles.Add(temp);
        }
    }
        // foreach (var tile in tiles) {
        //     Vector3 pos = new()
        //     {
        //         x = tile.Position().x,
        //         z = tile.Position().y
        //     };

        //     // GameObject tile = Instantiate(
        //     //         tilePallette[tileSetSave.tiles[i,j]],
        //     //         pos + (q * Mathf.Abs(r) % 4 * 0.1f * Vector3.up),
        //     //         Quaternion.identity,
        //     //         tileContainer
        //     //     );
        //     GameObject temp = Instantiate(
        //         moveHighlightPrefab,
        //         pos + (tile.q * Mathf.Abs(tile.r) % 4 * 0.2f *+0.4f * Vector3.up),
        //         Quaternion.identity,
        //         highlightBucket
        //     );
        //     moveHighlightTiles.Add(temp);
        // }
    // }
    [SerializeField] GameObject borderPrefab;
    [SerializeField] Transform borderBucket;
    public void CreateBorder() {
        List<HexCoord> tiles = new();
        foreach (I_Tile tileObj in gameTiles)
        {
            tiles.Add(tileObj.index);
        }
        if(tiles==null) {
            // edgeLineRenderer.positionCount=0;
            return;
        }
        List<Vector2> edges = new();
        List<float> angles = new();
        // List<HexCoord> coveredEdges = new();
        float minHeight = 2f;
        foreach (var tile in tiles) {
            int neighborNumb = 0;
            foreach (HexCoord neighbor in tile.Neighbors()) {
                if(!IsOnTile(neighbor)) {
                    Vector2 tempC1= tile.Corner((neighborNumb-1)%6);
                    float height1 = gameTiles[tile.q,tile.r + (tile.q >> 1)].height + 0.35f;
                    
                    float height2 = (neighbor.q * Mathf.Abs(neighbor.r) % 4 * 0.1f) + 0.35f;
                    height1 = height1 < height2 ? height1 : height2;
                    if(height1<minHeight) {
                        minHeight=height1;
                        // edges.Add(new Vector3(edges[^1].x,height1,edges[^1].z));
                    }
                    Vector2 tempC2= tile.Corner(neighborNumb%6);
                    Vector3 center = tempC2+((tempC1-tempC2)/2);
                    angles.Add(Mathf.Atan2(tempC1.x-tempC2.x,tempC1.y-tempC2.y+0.001f)*Mathf.Rad2Deg);
                    edges.Add(center);
                    // edges.Add(new Vector3(tempC.x,height1,tempC.y));
                    // height=height1;
                }
                neighborNumb++;
            }
        }
        // edgeLineRenderer.positionCount=edges.Count;
        // edgeLineRenderer.SetPositions(edges.ToArray());
        for (int i = 0;i < edges.Count;i++) {
            GameObject temp = Instantiate(
                borderPrefab,
                new (edges[i].x,minHeight,edges[i].y),
                Quaternion.Euler(0,angles[i],0),
                borderBucket
            );
            boarderGOs.Add(temp);
            // moveHighlightTiles.Add(temp);
        }
    }

    void Awake() {
        inst = this;
    }

    public static void SetInst() {
        inst = GameObject.FindFirstObjectByType<TileManager>();
    }

    void Start()
    {
        GenerateMap();
    }

    // public void SaveMap() {
    //     if(tempMap == null) {
    //         return;
    //     }
    //     SaveManager.inst.SetTileSave(new(tempMap,tempMap.arrays.Count, tempMap.arrays[0].cells.Count),currentMapID);
    // }

    // Using Hex Coordinates
    public void SetTile(I_Tile n_Tile, HexCoord tile) {
        int fixedX = tile.q;
        int fixedY = tile.r + (tile.q >> 1);
        if(!IsOnTile(tile) || n_Tile == gameTiles[fixedX,fixedY])
            return;
        Destroy(gameTiles[fixedX,fixedY].gameObject);
        gameTiles[fixedX,fixedY] = n_Tile;
    }

    public I_Tile GetTile(HexCoord tile) {
        int fixedX = tile.q;
        int fixedY = tile.r + (tile.q >> 1);
        if(!IsOnTile(tile)) {
            return null;
        }
        return gameTiles[fixedX,fixedY];
    }

    public bool IsOnTile(HexCoord tile)
    {
        if (tile.q < qMin || tile.q >= qMax)
            return false;
    
        int qOff = tile.q >> 1;
        int fixedY = tile.r + qOff;
        
        if (fixedY < 0 || fixedY >= tileHeight)
            return false;
    
        return true;
    }
    
    public List<I_Unit> GetUnitsOnNeighboringHexes(HexCoord centerHex)
    {
        List<I_Unit> neighbors = new List<I_Unit>();

        foreach (HexCoord neighborHex in centerHex.Neighbors())
        {
            I_Tile tile = GetTile(neighborHex);
            if (tile != null && tile.unitOnTile != null)
            {
                neighbors.Add(tile.unitOnTile);
            }
        }
        return neighbors;
    }
    [SerializeField] GameObject plainsCover;
    [SerializeField] GameObject swampCover;

    [Header("MapGenParamaters")]
    public bool editorUpdate = true;
    public float mountainHeight = .75f;
    public float marshHeight = 0f;
    public float plainsHeight = .4f;
    public float mineChance = .3f;
    public float coverChance = .35f;
    public float scale; 
    [Range(0,99)]
    public int octaves; 
    [Range(0,1)]
    public float persistance; 
    [Range(0,1)]
    public float lacunarity; 
    public Vector2 offset;
    public Vector2Int widthRange = new();
    public Vector2Int heightRange = new();
    [Header("RandomMapData")]
    public List<HexCoord> playerDeploy = new();
    public List<HexCoord> aiDeploy = new();
    List<GameObject> boarderGOs = new();
    List<GameObject> tileGOs = new();

    public void Clear() {
        playerDeploy = new();
        aiDeploy = new();
        foreach (GameObject border in boarderGOs) {
            DestroyImmediate(border);
        } 
        foreach (GameObject tile in tileGOs) {
            DestroyImmediate(tile);
        }
        tileGOs.Clear();
        boarderGOs.Clear();
    }
    
    public void GenerateMap(int seedShuffle = 0) {
        // Read in from map data
        playerDeploy = new();
        aiDeploy = new();
        foreach (GameObject border in boarderGOs) {
            DestroyImmediate(border);
        } 
        Vector3 pos = Vector3.zero;
        int id = currentMapID;
        if(randomMap) {
            int seed = 0;
            System.Random rand = new();
            if(SaveManager.inst != null) {
                seed = SaveManager.inst.playerSave.tileSeed*(SaveManager.inst.playerSave.locationOrder+1)+seedShuffle;
                rand = new(seed);
            }

            tileWidth = (rand.Next() % (widthRange.y-widthRange.x)) + widthRange.x;
            tileHeight = (rand.Next() % (heightRange.y-heightRange.x)) + heightRange.x;

            gameTiles = new I_Tile[tileWidth,tileHeight];
            

            qMax = tileWidth;
            qMin = 0;

            int randQ = rand.Next() % (qMax/3);
            int qOff = randQ >> 1;
            int randR = (rand.Next() % (tileHeight/2)) - qOff;

            HexCoord playerHome = new(randQ,randR);
            if(IsOnTile(playerHome)) {
                playerDeploy.Add(playerHome);
            } else {
                playerDeploy.Add(new(qMin,0 + (qMin >> 1)));
            }

            randQ = (rand.Next() % (qMax/3)) + qMax*(2/3);
            qOff = randQ >> 1;
            randR = (rand.Next() % (tileHeight/2)) + (tileHeight/2) - qOff;

            HexCoord aiHome = new(randQ,randR);
            if(IsOnTile(aiHome)) {
                aiDeploy.Add(aiHome);
            } else {
                aiDeploy.Add(new(qMax,tileHeight + qMax >> 1));
            }

            int escape = 0;
            for(int i =0;(i<SaveManager.inst.playerSave.units.Count || i<10) && escape<999;) {
                List<HexCoord> temp = playerDeploy[^1].Neighbors().ToList();
                int j = 0;
                while(j<6 && (i<SaveManager.inst.playerSave.units.Count || i<10)) {
                    if(IsOnTile(temp[j]) && !playerDeploy.Contains(temp[j])) {
                        playerDeploy.Add(temp[j]);
                        i++;
                    }
                    j++;
                }
                escape++;
            }
            escape=0;
            for(int i =0;i<10 && escape<999;) {
                List<HexCoord> temp = aiDeploy[^1].Neighbors().ToList();
                int j = 0;
                while(j<6 && i<10) {
                    if(IsOnTile(temp[j]) && !aiDeploy.Contains(temp[j]) && !playerDeploy.Contains(temp[j])) {
                        aiDeploy.Add(temp[j]);
                        i++;
                    }
                    j++;
                }


                if(aiDeploy[^1]==aiHome) {
                    randQ = (rand.Next() % (qMax/3)) + qMax*(2/3);
                    qOff = randQ >> 1;
                    randR = (rand.Next() % (tileHeight/2)) + (tileHeight/2) - qOff;

                    aiHome = new(randQ,randR);
                    aiDeploy.Clear();
                }
                escape++;
            }

            if(aiDeploy.Count<9 || playerDeploy.Count<9) {
                Debug.LogWarning("Undersized deploy area!");
                if(seedShuffle<400) {
                    GenerateMap(seedShuffle+10); // This is bad but oh well
                }
                return;
            }

            // Debug.Log(gameTiles.GetLength(0)+" "+gameTiles.GetLength(1));

            

            float[,] noise = Noise.GenerateNoiseMap(tileWidth,tileHeight,seed,scale,octaves,persistance,lacunarity,offset);
            foreach (HexCoord coord in playerDeploy) {
                noise[coord.q,coord.r + (coord.q >>1)]=plainsHeight+0.1f;
            }
            foreach (HexCoord coord in aiDeploy) {
                noise[coord.q,coord.r + (coord.q >>1)]=plainsHeight+0.1f;
            }
            for(float offset=0;offset>-1f;offset-=0.1f) {
                foreach (GameObject tile in tileGOs) {
                    DestroyImmediate(tile);
                }
                tileGOs.Clear();
                for(int q = 0, i=0; q < tileWidth; q++){
                    qOff = q >> 1;
                    for (int r = -qOff,j=0; r < tileHeight - qOff; r++){
                        HexCoord coord = new HexCoord(q, r);
                        int type = 0;
                        float height = noise[q,r+qOff] + offset;
                        if(height >= mountainHeight) {
                            type = 3;
                        } else if(height >= plainsHeight) {
                            type = 1;
                            if(rand.NextDouble()<mineChance && !playerDeploy.Contains(coord) && !aiDeploy.Contains(coord)) {
                                type=tilePallette.Count-1;
                            } else if(rand.NextDouble()<coverChance) {
                                type=-1;
                            }
                        } else{
                            type = 2;
                            if(rand.NextDouble()<coverChance) {
                                type=-2;
                            }
                        }
                        
                        pos.x = coord.Position().x;
                        pos.z = coord.Position().y;
                        height *= .33f;
                        float rotation = rand.Next() % 6 * 60f; 
                        GameObject prefab = null;
                        if(type==-1) {
                            prefab = plainsCover;
                        } else if(type==-2) {
                            prefab = swampCover;
                        }  else {
                           prefab = tilePallette[type];
                        }

                        GameObject tile = Instantiate(
                            prefab,
                            pos + height*Vector3.up,
                            Quaternion.Euler(0,rotation,0),
                            tileContainer
                        );

                        if(type<0) {
                            tile.GetComponent<CoverTile>().coverCenterDegrees=rotation+90;
                        }
                        
                        I_Tile temp = tile.GetComponent<I_Tile>();
                        temp.index = new HexCoord(q, r);
                        temp.height=height;
                        tile.name = $"Hex[{q},{r},{(-q - r)}]";
                        tile.transform.localScale = Vector3.one * 2;

                        gameTiles[q, r + qOff] = temp;
                        tileGOs.Add(tile);
                        j++;
                    }
                    i++;
                }
                if(Pathfinding.FindPath(playerHome,aiHome,true).Count>0) {
                    break;
                }
            }
            CreateBorder();
        }
    }
}
