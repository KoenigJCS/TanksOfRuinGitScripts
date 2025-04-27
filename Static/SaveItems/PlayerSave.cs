using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public struct PlayerSave
{
    public int locationOrder;
    public List<ItemSave> items;
    public List<UnitSave> units;
    public List<LocationSave> mapInfo;
    public int curentScene;
    public int tileSeed;
    public int currentDifficulty;
    public int levelCount;
    public PlayerSave(int n_locationOrder = -1) {
        locationOrder = n_locationOrder;
        items=new();
        mapInfo=new();
        units=new();
        curentScene=-1;
        tileSeed=-1;
        currentDifficulty=-1;
        levelCount=0;
    }

    public PlayerSave(int n_locationOrder = -1,List<LocationSave> n_mapInfo = null) {
        locationOrder = n_locationOrder;
        items=new();
        units=new();
        mapInfo=n_mapInfo;
        curentScene=-1;
        tileSeed=-1;
        currentDifficulty=-1;
        levelCount=0;
    }
}