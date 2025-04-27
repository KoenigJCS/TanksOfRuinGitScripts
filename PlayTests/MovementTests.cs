using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Settworks.Hexagons;
using UnityEngine.Tilemaps;

public class MovementTests
{
    // A Test behaves as an ordinary method
    private GameObject managers;
    private I_Unit unit1;
    private I_Unit unit2;
    // [Test]
    // public void NewTestScriptSimplePasses()
    // {
        
    // }
    [SetUp]
    public void Setup() {
        GameObject managers = GameObject.Instantiate(new GameObject());
        TileManager t1 = managers.AddComponent<TileManager>();
        TileManager.SetInst();
        t1.debugFlag=true;
        t1.GenerateMap(); // BROKEN LMAO
        TileManager.inst.GetTile(new(0,1)).moveCost=999; // Making Mountain
        // TileManager.inst.GetTile(new(2,2)).moveCost=999; // Making Mountain
        TileManager.inst.GetTile(new(3,3)).moveCost=999; // Making Mountain
        GameObject unitGO1 = GameObject.Instantiate(new GameObject());
        unit1 = unitGO1.AddComponent<BasicUnit>();
        unit1.maxActionPoints=3;
        unit1.actionPoints=3;
        GameObject unitGO2 = GameObject.Instantiate(new GameObject());
        unit2 = unitGO2.AddComponent<BasicUnit>();
        unit2.playerControlled=false;
        unit2.hexPosition = new(1,0);
        TileManager.inst.GetTile(new(1,0)).unitOnTile=unit2;
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [Test]
    public void Movement_BasicMove() {
        Assert.AreEqual(Pathfinding.FindPath(new HexCoord(0,0),new HexCoord(1,1),true).Count,2,"Movement Working Incorrectly");
    }

    [Test]
    public void Movement_MountainBlockingMove() {
        Assert.IsTrue(Pathfinding.FindPath(new HexCoord(0,0),new HexCoord(0,1),true).Count==0,"Mountain Movement Blocking Working Incorrectly");
    }

    [Test]
    public void Movement_EnemyBlockingMove() {
        Assert.IsTrue(Pathfinding.FindPath(new HexCoord(0,0),new HexCoord(1,0),true).Count==0,"Enemy Movement Blocking Working Incorrectly");
    }
    
    [Test]
    public void Movement_PathAround() {
        Assert.AreEqual(Pathfinding.FindPath(new HexCoord(0,0),new HexCoord(0,2),true).Count,3,$"Movement Repathing Working Incorrectly");
    }

}
