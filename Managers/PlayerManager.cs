using System.Collections.Generic;
using System.Linq;
using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager inst;
    [SerializeField] List<I_Unit> units; 
    private Dictionary<HexCoord, int> reachableTiles;
    public List<I_Unit> Units { get { return units; } }
    I_Unit selectedUnit;
    bool playerTurn = true;
    public bool uILock = false;
    void Awake() {
        inst = this;
    }

    private void Start() {
        selectedUnit = null;
        units = new();
    }
    public void AddUnit(I_Unit n_unit) {
        units.Add(n_unit);
    }
    public void RemoveUnit(I_Unit n_unit) {
        if(units.Contains(n_unit)) {
            units.Remove(n_unit);
        }
    }

    public void OnTurnStart() {
        print("Starting");
        playerTurn=true;
        foreach (I_Unit unit in units) {
            unit.actionPoints = unit.maxActionPoints;
            unit.firePoints = unit.maxFirePoints;
            unit.hasBeenHit = false;
        }
    }

    public void EndTurn() {
        EndTurn(false);
    }

    public void EndTurn(bool certain) {
        if(uILock) {
            return;
        }
        if(certain) {
            playerTurn=false;
            SelectUnit(null);
            UIManager.inst.SetCertantyDisplay(false);
            AIManager.inst.OnTurnStart();
            return;
        } else {
            if(CheckRemainingMoves()) {
                UIManager.inst.SetCertantyDisplay(true);
            } else {
                EndTurn(true);
            }
        }
    }

    bool CheckRemainingMoves() {
        foreach (I_Unit unit in units) {
            // Add all remaning things a unit can do here.
            if(unit.actionPoints>0 || false) {
                return true;
            }
        }
        return false;
    }
    public void SelectUnit(I_Unit unit) {
        if(selectedUnit) {
            selectedUnit.isSelected=false;
            reachableTiles = null;
        }
        selectedUnit = unit;
        if(!unit) return;

        unit.isSelected=true;

        reachableTiles = Pathfinding.inst.FindReachableTiles(unit.hexPosition, unit.actionPoints,unit,unit.playerControlled);
    }


    public void Update() {
        if (playerTurn &&  !uILock && Input.GetMouseButtonDown (0)) { 
            RaycastHit hit; 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            bool hasHit = Physics.Raycast (ray,out hit, 100.0f);
            if (hasHit) { 
                I_Unit unit;
                if(EventSystem.current.IsPointerOverGameObject()) {
                    // UI interactions
                } else if(
                    hit.transform!=null 
                    && (unit = hit.transform.gameObject.GetComponent<I_Unit>())
                    || (unit = UnitAt(HexCoord.AtPosition(new(hit.point.x,hit.point.z))))
                ) { 
                    UIManager.inst.SetDisplayUnit(unit);
                    if(selectedUnit && !unit.playerControlled && AIManager.inst.FindTargets(selectedUnit,true).Contains(unit) && selectedUnit.firePoints>0) {
                        selectedUnit.firePoints--;
                        selectedUnit.Fire(unit);
                        SelectUnit(null);
                    } else if(unit.playerControlled && (unit.actionPoints>0 || unit.firePoints>0)) {
                        SelectUnit(unit);
                    }
                } else if(selectedUnit && selectedUnit.playerControlled) {
                    HexCoord clickedCoord = HexCoord.AtPosition(new(hit.point.x,hit.point.z));
                    if (reachableTiles != null && reachableTiles.ContainsKey(clickedCoord))
                    {
                        MoveUnit(selectedUnit, clickedCoord, true);
                    }
                    else
                    {
                        SoundManager.inst.PlayBadButton();
                    }
                } else {
                    UIManager.inst.SetDisplayUnit(null);
                }
            } else {
                UIManager.inst.SetDisplayUnit(null);
            }
        }
    }

    private void MoveUnit(I_Unit unit, HexCoord dest,bool playerTeam)
    {
        int moveCost = reachableTiles[dest];
        if (unit.actionPoints >= moveCost)
        {
            unit.Move(dest);
            unit.actionPoints -= moveCost;
            
            reachableTiles = Pathfinding.inst.FindReachableTiles(unit.hexPosition, unit.actionPoints, unit,playerTeam);
        }
    }

    public void HighlightReachableTiles() {
        //Todo
    }


    public I_Unit UnitAt(HexCoord pos) {
        foreach (I_Unit unit in units) {
            if(unit.hexPosition==pos) {
                return unit;
            }
        }
        return AIManager.inst.UnitAt(pos);
    }
}