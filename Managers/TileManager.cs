using System.Collections.Generic;
using UnityEngine;
using Settworks.Hexagons;
using System;
using Unity.Mathematics;

public class TileManager : MonoBehaviour {
    [SerializeField] I_Tile[,] gameTiles;
    public static TileManager inst;
    [SerializeField] Transform tileContainer;
    [SerializeField] int tileWidth;
    [SerializeField] int tileHeight;
    
    // This should automatically read in from the file of tiles in an ideal world.
    [SerializeField] List<GameObject> tilePallette;
    int qMin = 0;
    int qMax = int.MinValue;
    [SerializeField] int currentMapID = 0;
    [SerializeField] int[,] tileArray;
    [Header("Debug")]
    [SerializeField] bool saveTempMap = false;  
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

    void Awake() {
        inst = this;
    }

    void Start()
    {
        if(saveTempMap) {
            SaveMap();
        }
        GenerateMap();
    }

    public void SaveMap() {
        if(tempMap == null) {
            return;
        }
        SaveManager.inst.SetTileSave(new(tempMap,tempMap.arrays.Count, tempMap.arrays[0].cells.Count),currentMapID);
    }

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


    public void GenerateMap() {
        // Read in from map data

        Vector3 pos = Vector3.zero;
        int id = currentMapID;
        if(randomMap) {
            System.Random rand = new();
            // Update this lmao;
            id = rand.Next() % 4;
        }
        
        // Used to create the map. 1 = Plains, 2 = Forest, 3 = Mountain.
        TileSetSave tileSetSave = SaveManager.inst.GetTileSave(id);
        gameTiles = new I_Tile[tileSetSave.width,tileSetSave.height];
        tileWidth = tileSetSave.width;
        tileHeight = tileSetSave.height;
        
        qMax = tileWidth;
        qMin = 0;

        for(int q = 0, i=0; q < tileWidth; q++){
            int qOff = q >> 1;
            for (int r = -qOff,j=0; r < tileHeight - qOff; r++){
                HexCoord coord = new HexCoord(q, r);
                
                pos.x = coord.Position().x;
                pos.z = coord.Position().y;

                GameObject tile = Instantiate(
                    tilePallette[tileSetSave.tiles[i,j]],
                    pos + (q * Mathf.Abs(r) % 4 * 0.1f * Vector3.up),
                    Quaternion.identity,
                    tileContainer
                );
                
                tile.GetComponent<I_Tile>().index = new HexCoord(q, r);
                tile.name = $"Hex[{q},{r},{(-q - r)}]";
                tile.transform.localScale = Vector3.one * 2;

                gameTiles[q, r + qOff] = tile.GetComponent<I_Tile>();
                j++;
            }
            i++;
        }
    }
}
