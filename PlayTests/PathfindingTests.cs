using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Settworks.Hexagons;
using UnityEngine.Tilemaps;

public class PathfindingTests
{
    private GameObject managers;
    private I_Unit unit1;
    private I_Unit unit2;

    [SetUp]
    public void Setup() {
        GameObject managers = GameObject.Instantiate(new GameObject());
        TileManager t1 = managers.AddComponent<TileManager>();
        TileManager.SetInst();
        t1.debugFlag=true;
        t1.GenerateMap();
        TileManager.inst.GetTile(new(1,1)).moveCost=999; // Making Mountain
        
        GameObject unitGO1 = GameObject.Instantiate(new GameObject());
        unit1 = unitGO1.AddComponent<BasicUnit>();
        unit1.weaponRange=7;
        GameObject unitGO2 = GameObject.Instantiate(new GameObject());
        unit2 = unitGO2.AddComponent<BasicUnit>();
        unit2.playerControlled=false;
    }

    [Test]
    public void Pathfinding_LineOfSight() {
        Assert.IsTrue(Pathfinding.LineOfSight(new HexCoord(0,0),new HexCoord(0,1),unit2,unit1,true),"Line of Sight Working Incorrectly");
    }

    [Test]
    public void Pathfinding_LineOfSight_Range() {
        Assert.IsFalse(Pathfinding.LineOfSight(new HexCoord(0,0),new HexCoord(0,8),unit2,unit1,true),"Line of Sight Range Working Incorrectly");
    }

    [Test]
    public void Pathfinding_LineOfSight_Mountains() {
        Assert.IsFalse(Pathfinding.LineOfSight(new HexCoord(0,0),new HexCoord(2,2),unit2,unit1,true),"Line of Sight Mountains Working Incorrectly");
    }

}
