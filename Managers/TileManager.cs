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
