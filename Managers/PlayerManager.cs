using System.Collections.Generic;
using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager inst;
    [SerializeField] List<I_Unit> units; 
    public List<I_Unit> Units { get { return units; } }
    I_Unit selectedUnit;
    bool playerTurn = true;
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
            unit.hasBeenHit = false;
        }
    }

    public void EndTurn() {
        EndTurn(false);
    }

    public void EndTurn(bool certain) {
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
        }
        selectedUnit = unit;
        if(!unit) return;

        unit.isSelected=true;
        AIManager.inst.ClearTargets();
        AIManager.inst.HighlightTargets(unit);
    }

    public void Update() {
        if (playerTurn && Input.GetMouseButtonDown (0)) { 
            RaycastHit hit; 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            bool hasHit = Physics.Raycast (ray,out hit, 100.0f);
            if (hasHit) { 
                I_Unit unit;
                if(EventSystem.current.IsPointerOverGameObject()) {
                    
                } else if(
                    hit.transform!=null 
                    && (unit = hit.transform.gameObject.GetComponent<I_Unit>())
                    || (unit = UnitAt(HexCoord.AtPosition(new(hit.point.x,hit.point.z))))
                ) { 
                    UIManager.inst.SetDisplayUnit(unit);
                    if(selectedUnit && !unit.playerControlled) {
                        selectedUnit.Fire(unit);
                        SelectUnit(null);
                    } else if(unit.playerControlled) {
                        SelectUnit(unit);
                    }
                } else if(selectedUnit && selectedUnit.playerControlled) {
                    selectedUnit.Move(HexCoord.AtPosition(new(hit.point.x,hit.point.z)));
                } else {
                    UIManager.inst.SetDisplayUnit(null);
                }
            } else {
                UIManager.inst.SetDisplayUnit(null);
            }
        }
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