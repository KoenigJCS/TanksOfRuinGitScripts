using System;
using System.Collections.Generic;
using System.Linq;
using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager inst;
    [SerializeField] List<I_Unit> units;
    void Awake() {
        inst = this;
    }

    private void Start() {
        units = new List<I_Unit>();
    }
    public void AddUnit(I_Unit n_unit) {
        units.Add(n_unit);
    }
    
    public void RemoveUnit(I_Unit n_unit) {
        if(units.Contains(n_unit)) {
            units.Remove(n_unit);
        }
    }

    public void CheckTeamDead() {
        if(units.Count == 0) {
            UIManager.inst.EndSceneVictory();
        }
    }

    // AITurn function written by Tommy Smith
    // Commented code was previous edition and should still be considered
    public void AITurn()
    {
        /*Debug.Log("AI Turn Started.");
        if(units == null || units.Count == 0)
        {
            Debug.Log("No AI units to process.");
            EndTurn();
            return;
        }

        foreach (I_Unit enemyUnit in units)
        {
            Debug.Log($"Processing enemy unit: {enemyUnit.name}");

            if (enemyUnit.actionPoints <= 0) {
                Debug.Log($"Unit {enemyUnit.name} has no action points left.");
                continue;
            }

            I_Unit nearestPlayer = GetNearestPlayerUnit(enemyUnit);
            if (nearestPlayer == null)
            {
                Debug.LogWarning($"No nearest player unit found for enemy unit {enemyUnit.name}.");
                continue;
            }

            if (Pathfinding.inst == null)
            {
                Debug.LogError("Pathfinding.inst is null.");
                continue;
            }

            Debug.Log($"Finding path from {enemyUnit.hexPosition} to {nearestPlayer.hexPosition}");
            HexCoord dest = new();
            float shortestDistance = float.MaxValue;
            float currentDistance;
            foreach (HexCoord neighbor in nearestPlayer.hexPosition.Neighbors())
            {   
                I_Tile tile = TileManager.inst.GetTile(neighbor);
                if(tile == null) {
                    continue;
                }
                if(tile.unitOnTile == null && tile.moveCost<100) {
                    currentDistance = HexCoord.Distance(enemyUnit.hexPosition, neighbor);
                    if (currentDistance < shortestDistance)
                    {
                        shortestDistance = currentDistance;
                        dest = tile.index;
                    }
                }
            }
            List<HexCoord> path = Pathfinding.inst.FindPath(enemyUnit.hexPosition, dest, enemyUnit);
            
            if (path == null || path.Count == 0)
            {
                Debug.LogWarning($"No path found for enemy unit {enemyUnit.name} to target {nearestPlayer.name}.");
                continue;
            }

            Debug.Log($"Path found for unit {enemyUnit.name}: {string.Join(" -> ", path)}");

            int remainingAP = enemyUnit.actionPoints;
            List<HexCoord> positionsToMove = new List<HexCoord>();
            

            for (int index = 1; index < path.Count && remainingAP > 0; index++)
            {
                HexCoord step = path[index];
                I_Tile tile = TileManager.inst.GetTile(step);
                if (tile == null)
                {
                    Debug.LogWarning($"Tile at {step} is null. Stopping movement for unit {enemyUnit.name}.");
                    break;
                }

                int moveCost = tile.moveCost;
                if (moveCost > remainingAP)
                {
                    Debug.Log($"Not enough AP to move to {step} for unit {enemyUnit.name}. Required: {moveCost}, Available: {remainingAP}");
                    break;
                }

                positionsToMove.Add(step);
                remainingAP -= moveCost;
            }

            foreach (HexCoord position in positionsToMove)
            {
                Debug.Log($"Unit {enemyUnit.name} moving to {position}");
                enemyUnit.Move(position);
                enemyUnit.actionPoints -= TileManager.inst.GetTile(position).moveCost;
            }

            I_Unit[] targets = FindPlayerTargets(enemyUnit);
            if (targets.Length > 0)
            {
                Debug.Log($"Unit {enemyUnit.name} firing at {targets[0].name}");
                enemyUnit.Fire(targets[0]);
            }
            else
            {
                Debug.Log($"Unit {enemyUnit.name} found no targets to fire at.");
            }
        }

        Debug.Log("AI Turn Ended.");
        EndTurn();*/

        bool enemyUnitFire(I_Unit enemyUnit, I_Unit[] targetsInRange) {
            if(targetsInRange.Length == 0) {
                return false;
            }
            Debug.Log($"Unit {enemyUnit.name} firing at {targetsInRange[0].name}");
            enemyUnit.Fire(targetsInRange[0]);
            enemyUnit.firePoints -= 1;
            return true;
        }
        
        Debug.Log("AI Turn Started.");
        if(units == null || units.Count == 0)
        {
            Debug.Log("No AI units to process.");
            EndTurn();
            return;
        }

        foreach (I_Unit enemyUnit in units)
        {
            Debug.Log($"Processing enemy unit: {enemyUnit.name}");

            if (enemyUnit.actionPoints <= 0 && enemyUnit.firePoints <= 0) {
                Debug.Log($"Unit {enemyUnit.name} has no action points left.");
                continue;
            }

            I_Unit[] targetsInRange = FindPlayerTargets(enemyUnit);
            if (targetsInRange.Length > 0 && enemyUnit.firePoints >= 1)
            {
                if(enemyUnitFire(enemyUnit,targetsInRange)) {
                    continue;
                }
            }

            I_Unit nearestPlayer = GetNearestPlayerUnit(enemyUnit);
            if (nearestPlayer == null)
            {
                Debug.LogWarning($"No nearest player unit found for enemy unit {enemyUnit.name}.");
                continue;
            }

            if (Pathfinding.inst == null)
            {
                Debug.LogError("Pathfinding.inst is null.");
                continue;
            }
            
            HexCoord dest = new();
            float shortestDistance = float.MaxValue;

            foreach (HexCoord neighbor in nearestPlayer.hexPosition.Neighbors())
            {   
                I_Tile tile = TileManager.inst.GetTile(neighbor);
                if(tile == null) {
                    continue;
                }
                if(tile.unitOnTile == null && tile.moveCost < 100)
                {
                    float currentDistance = HexCoord.Distance(enemyUnit.hexPosition, neighbor);
                    if (currentDistance < shortestDistance)
                    {
                        shortestDistance = currentDistance;
                        dest = tile.index;
                    }
                }
            }
            
            if (Math.Abs(shortestDistance - float.MaxValue) < 0.000001f) // checks rough equality of floats
            {
                Debug.LogWarning($"No valid destination found for enemy unit {enemyUnit.name}.");
                continue;
            }else{
                Debug.Log($"Destination for enemy unit {enemyUnit.name} is {dest}");
            }

            List<HexCoord> path = Pathfinding.inst.FindPath(enemyUnit.hexPosition, dest, enemyUnit, enemyUnit.playerControlled);
            if (path == null || path.Count == 0)
            {
                Debug.LogWarning($"No path found for enemy unit {enemyUnit.name} to target {nearestPlayer.name}.");
                continue;
            }

            int remainingAP = enemyUnit.actionPoints;
            List<HexCoord> positionsToMove = new List<HexCoord>();

            for (int index = 1; index < path.Count && remainingAP > 0; index++)
            {
                HexCoord step = path[index];
                I_Tile tile = TileManager.inst.GetTile(step);
                if (tile == null)
                {
                    Debug.LogWarning($"Tile at {step} is null. Stopping movement for unit {enemyUnit.name}.");
                    break;
                }

                int moveCost = tile.moveCost;
                if (moveCost > remainingAP)
                {
                    Debug.Log($"Not enough AP to move to {step} for unit {enemyUnit.name}. Required: {moveCost}, Available: {remainingAP}");
                    break;
                }

                Debug.Log($"AddingPos: {step.ToString()}");
                positionsToMove.Add(step);
                remainingAP -= moveCost;
            }
            
            HexCoord destination = positionsToMove[positionsToMove.Count - 1];
            I_Tile destTile = TileManager.inst.GetTile(destination);

            if (destTile.unitOnTile != null)
            {
                positionsToMove.RemoveAt(positionsToMove.Count - 1);
            }

            foreach (HexCoord position in positionsToMove)
            {
                Debug.Log($"Unit {enemyUnit.name} moving to {position}");
                enemyUnit.Move(position);
                enemyUnit.actionPoints -= TileManager.inst.GetTile(position).moveCost;
            }

            targetsInRange = FindPlayerTargets(enemyUnit);
            if (targetsInRange.Length > 0 && enemyUnit.firePoints >= 1)
            {
                if(enemyUnitFire(enemyUnit,targetsInRange)) {
                    continue;
                }
            }
        }

        Debug.Log("AI Turn Ended.");
        EndTurn();
    }

    public void EndTurn() {
        PlayerManager.inst.OnTurnStart();
    }

    public void OnTurnStart() {
        foreach (I_Unit unit in units) {
            unit.actionPoints = unit.maxActionPoints;
            unit.firePoints = unit.maxFirePoints;
            unit.hasBeenHit = false;
        }
        AITurn();
    }
    
    public I_Unit UnitAt(HexCoord pos) {
        foreach (I_Unit unit in units) {
            if(unit.hexPosition == pos) {
                return unit;
            }
        }
        return null;
    }

    public I_Unit[] FindTargets(I_Unit sourceUnit, bool playerTeam) {
        List<I_Unit> targets = new List<I_Unit>();
        foreach (I_Unit unit in units) {
            int diff = HexCoord.Distance(sourceUnit.hexPosition,unit.hexPosition);
            if(diff <= sourceUnit.weaponRange && Pathfinding.inst.LineOfSight(sourceUnit.hexPosition,unit.hexPosition,unit,playerTeam)) {
                targets.Add(unit);
            }
        }
        return targets.ToArray();
    }

    I_Unit GetNearestPlayerUnit(I_Unit enemyUnit)
    {
        Debug.Log("Finding nearest player unit.");
        I_Unit nearestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (I_Unit playerUnit in PlayerManager.inst.Units) {
            float distance = HexCoord.Distance(enemyUnit.hexPosition, playerUnit.hexPosition);
            Debug.Log($"Distance from {enemyUnit.name} to {playerUnit.name}: {distance}");
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPlayer = playerUnit;
            }
        }

        if(nearestPlayer != null)
            Debug.Log($"Nearest player unit to {enemyUnit.name} is {nearestPlayer.name} at distance {minDistance}");
        else
            Debug.LogWarning($"No player units found for {enemyUnit.name}.");

        return nearestPlayer;
    }

    public I_Unit[] FindPlayerTargets(I_Unit sourceUnit) {
        List<I_Unit> targets = new List<I_Unit>();
        foreach (I_Unit unit in PlayerManager.inst.Units)
        {
            int distance = HexCoord.Distance(sourceUnit.hexPosition, unit.hexPosition);
            if(distance <= sourceUnit.weaponRange && Pathfinding.inst.LineOfSight(sourceUnit.hexPosition,unit.hexPosition,unit,false)) {
                targets.Add(unit);
            }
        }
        return targets.ToArray();
    }
    
    public void HighlightTargets(I_Unit sourceUnit, bool playerTeam) {
        I_Unit[] targets = FindTargets(sourceUnit,playerTeam);

        foreach (I_Unit unit in targets) {
            unit.SetModelLayer(10); 
        }
    }

    public void ClearTargets() {
        foreach (I_Unit unit in units) {
            unit.SetModelLayer(0); 
        }
    }
}
