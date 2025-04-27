using System;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

public class AIManager : MonoBehaviour {
    public static AIManager inst;
    [SerializeField] List<I_Unit> units;
    private int aiIndex = 0;
    [SerializeField] List<AIArmy> armies;
    public List<I_Unit> Units { get { return units; } }

    [SerializeField] private float rotateSpeed = 10f;

    private bool isRotating    = false;
    private bool enemyIsMoving = false;

    private I_Unit rotatingAttacker;
    private I_Unit rotatingTarget;
    void Awake() {
        inst = this;
    }
    private Quaternion attackerDestination;

    private void Start() {
        if (inst == null) inst = this;
        units = new List<I_Unit>();
    }
    private Quaternion targetDestination;
    bool aiTurn = false;

    public void StartBattle() {
        foreach (GameObject unitGO in ItemManager.inst.aiUnits) {
            I_Unit unit = unitGO.GetComponent<I_Unit>();
            unit.hexPosition=TileManager.inst.aiDeploy[^1];
            TileManager.inst.aiDeploy.RemoveAt(TileManager.inst.aiDeploy.Count-1);
            unit.Init();
        }
    }

    public AIArmy FindAIArmy(Random rand) {
        int strength = ItemManager.inst.playerUnits.Count*75;
        strength+= SaveManager.inst.playerSave.currentDifficulty*40;
        strength+= SaveManager.inst.playerSave.levelCount*10;
        strength+=rand.Next()%100;
        AIArmy foundArmy = armies[^1];
        for(int i = armies.Count-1;i>=0;i--) {
            foundArmy = armies[i];
            if(foundArmy.difficulty<=strength) {
                break;
            }
        }
        Debug.Log(strength);
        Debug.Log(ItemManager.inst.playerUnits.Count*75);
        Debug.Log(foundArmy.difficulty);
        return foundArmy;
    }
    public void AddUnit(I_Unit n_unit) {
        units.Add(n_unit);
    }
    
    public void RemoveUnit(I_Unit n_unit) {
        if (units.Contains(n_unit))
            units.Remove(n_unit);
    }
    
    public void AITurn() {
        if( aiIndex==99 || !aiTurn) {
            return;
        }

        if (aiIndex >= units.Count) {
            EndTurn();
            return;
        }
        if(enemyIsMoving || isRotating) {
            return;
        }
        I_Unit enemyUnit = units[aiIndex];
        DoAction(enemyUnit);
    }
    public void EndTurn() {
        foreach (I_Unit unit in units){
            unit.OnTurnEnd();
        }
        foreach (I_Unit unit in Units) {
            if (unit.repairmanPresent) {
                unit.health += 5f;
                if (unit.health > unit.maxHealth) {
                    unit.health = unit.maxHealth;
                }
            }
        }
        aiIndex = 99;
        aiTurn = false;
        PlayerManager.inst.OnTurnStart();
    }

    public void OnTurnStart() {
        aiIndex=0;
        aiTurn = true;
        foreach (I_Unit unit in units) {
            unit.actionPoints = unit.maxActionPoints;
            unit.firePoints   = unit.maxFirePoints;
            unit.hasBeenHit   = false;

            if (unit.frozen) {
                unit.actionPoints--;
                unit.frozen = false;
            }
            if (unit.radiationCounter > 0) {
                unit.health -= 2;
                unit.radiationCounter--;
            }
            if (unit.radiationCounter <= 0) {
                unit.radiationCounter = 0;
            }
            if (unit.shrapnelJam) {
                unit.firePoints--;
                unit.shrapnelJam = false;
            }
        }
        aiIndex = 0;
        AITurn();
    }

    public I_Unit[] FindPlayerTargets(I_Unit sourceUnit) {
        List<I_Unit> targets = new List<I_Unit>();
        foreach (I_Unit unit in PlayerManager.inst.Units) {
            int distance = HexCoord.Distance(sourceUnit.hexPosition, unit.hexPosition);
            if(distance <= sourceUnit.weaponRange && Pathfinding.LineOfSight(sourceUnit.hexPosition,unit.hexPosition,sourceUnit,unit,false)) {
                targets.Add(unit);
            }
        }
        return targets.ToArray();
    }
    
    private void DoAction(I_Unit unit) {
        bool moved = TryMoveUnit(unit);

        if (!moved && !enemyIsMoving && unit.firePoints>0) {
            DoFiring(unit);
        }
    }
    private bool TryMoveUnit(I_Unit enemyUnit) {
        I_Unit nearest = GetNearestPlayerUnit(enemyUnit);
        if (nearest == null || enemyUnit.actionPoints == 0 || HexCoord.Distance(nearest.hexPosition,enemyUnit.hexPosition) == 1) {
            return false; 
        }
        
        float shortestDistance = float.MaxValue;
        HexCoord desiredTile = enemyUnit.hexPosition;
        foreach (HexCoord neighbor in nearest.hexPosition.Neighbors()) {
            I_Tile tile = TileManager.inst.GetTile(neighbor);
            if (tile == null) continue;

            if (tile.unitOnTile == null && tile.moveCost < 100)  {
                float dist = HexCoord.Distance(enemyUnit.hexPosition, neighbor);
                if (dist < shortestDistance) {
                    shortestDistance = dist;
                    desiredTile = neighbor;
                }
            }
        }

        if (desiredTile == enemyUnit.hexPosition) {
            return false;
        }

        List<HexCoord> finalPath = Pathfinding.FindPath(enemyUnit.hexPosition, desiredTile, enemyUnit.playerControlled);
        if (finalPath == null || finalPath.Count < 1)
            return false;

        int moveCost = 0;
        for (int i = 0; i < finalPath.Count; i++) {
            I_Tile tile = TileManager.inst.GetTile(finalPath[i]);
            if (tile == null) break;
            if (moveCost + tile.moveCost > enemyUnit.actionPoints) break;
            moveCost += tile.moveCost;
        }

        if (moveCost == 0)
            return false;

        finalPath.Insert(0,enemyUnit.hexPosition);

        if (!enemyUnit.TryGetComponent(out SplineMovement mover))
            mover = enemyUnit.gameObject.AddComponent<SplineMovement>();

        enemyIsMoving = true;
        enemyUnit.actionPoints = 0; // Dont' Move Again

        mover.onMovementComplete = () => {

            HexCoord lastTile = finalPath[^1];
            enemyUnit.Move(lastTile);

            enemyIsMoving = false;

            mover.onMovementComplete = () => {;};

            DoFiring(enemyUnit);
        };

        
        mover.BeginSplineMovement(finalPath);
        return true; 
    }

    private void DoFiring(I_Unit enemyUnit) {
        if (enemyUnit.firePoints <= 0) {
            aiIndex++;
            AITurn();
            return;
        }

        I_Unit target = PickTargetFor(enemyUnit);
        if (target == null) {
            aiIndex++;
            AITurn();
            return;
        }
        if(!isRotating) {
            BeginRotateAndFire(enemyUnit, target);
        }
    }


    private void BeginRotateAndFire(I_Unit attacker, I_Unit target) {
        rotatingAttacker = attacker;
        rotatingTarget   = target;

        Vector3 dirA = target.transform.position - attacker.transform.position;
        dirA.y = 0;
        attackerDestination = Quaternion.LookRotation(dirA);

        if (!target.hasBeenHit) {
            Vector3 dirT = (attacker.transform.position - target.transform.position);
            dirT.y = 0;
            targetDestination = Quaternion.LookRotation(dirT);
        }
        
        attacker.firePoints-=1;

        isRotating = true; 
    }

    private bool RotateUnitTowards(I_Unit unit, Quaternion destination) {
        Quaternion current = unit.model.transform.rotation;
        Quaternion next = Quaternion.Slerp(current, destination, rotateSpeed * Time.deltaTime);
        unit.model.transform.rotation = next;

        float angleDiff = Quaternion.Angle(next, destination);
        if (angleDiff < 0.1f) {
            unit.model.transform.rotation = destination;
            return true;
        }
        return false;
    }

    private void Update() {
        if (enemyIsMoving || aiIndex==99 || !aiTurn) return;

        if (isRotating) {
            bool aDone = RotateUnitTowards(rotatingAttacker, attackerDestination);
            bool tDone = true;
            if (rotatingTarget && !rotatingTarget.hasBeenHit)
                tDone = RotateUnitTowards(rotatingTarget, targetDestination);

            if (aDone && tDone) {
                isRotating = false;

                rotatingAttacker.Fire(rotatingTarget);
                // rotatingAttacker.firePoints--;

                rotatingAttacker = null;
                rotatingTarget   = null;

                aiIndex++;
                AITurn();
            }
        }
    }

    private I_Unit PickTargetFor(I_Unit aiUnit) {
        I_Unit bestTarget = null;
        float bestDist = float.MaxValue;

        foreach (I_Unit candidate in PlayerManager.inst.Units) {
            float dist = HexCoord.Distance(aiUnit.hexPosition, candidate.hexPosition);
            if (dist <= aiUnit.weaponRange) {
                if (Pathfinding.LineOfSight(aiUnit.hexPosition, candidate.hexPosition, aiUnit, candidate, false)) {
                    if (dist < bestDist) {
                        bestDist = dist;
                        bestTarget = candidate;
                    }
                }
            }
        }
        Debug.Log($"[AI] Final picked target = {bestTarget?.name ?? "None"}");
        return bestTarget;
    }
    
    public void HighlightTargets(I_Unit sourceUnit, bool playerTeam) {
        I_Unit[] targets = FindTargets(sourceUnit,playerTeam);

        foreach (I_Unit unit in targets) {
            unit.SetModelLayer(10); 
        }
    }

    public void CheckWin() {
        if(units.Count==0) {
            PlayerManager.inst.uILock=true;
            UIManager.inst.GameWin();
        }
    }

    public I_Unit UnitAt(HexCoord pos) {
        foreach (I_Unit unit in units) {
            if (unit.hexPosition == pos)
                return unit;
        }
        return null;
    }

    private I_Unit GetNearestPlayerUnit(I_Unit aiUnit) {
        I_Unit nearest = null;
        float minDist = float.MaxValue;
        foreach (I_Unit playerU in PlayerManager.inst.Units) {
            float d = HexCoord.Distance(aiUnit.hexPosition, playerU.hexPosition);
            if (d < minDist) {
                minDist = d;
                nearest = playerU;
            }
        }
        return nearest;
    }

    public void ClearTargets() {
        foreach (I_Unit unit in units) {
            unit.SetModelLayer(0); 
        }
    }

    public I_Unit[] FindTargets(I_Unit sourceUnit, bool playerTeam) {
        List<I_Unit> targets = new List<I_Unit>();
        foreach (I_Unit unit in units) {
            int diff = HexCoord.Distance(sourceUnit.hexPosition,unit.hexPosition);
            if(diff <= sourceUnit.weaponRange && Pathfinding.LineOfSight(sourceUnit.hexPosition,unit.hexPosition,sourceUnit,unit,playerTeam)) {
                targets.Add(unit);
            }
        }
        return targets.ToArray();
    }
}
