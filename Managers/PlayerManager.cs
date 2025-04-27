using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum FireSequenceState
{
    None,
    AttackerRotating,
    TargetRotating,
    Firing
}

public enum MovementSequenceState
{
    None,
    Moving
}

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager _inst = null;
    public static PlayerManager inst
    {
        get
        {
            if (_inst == null)
                _inst = FindObjectOfType(typeof(PlayerManager)) as PlayerManager;

            return _inst;
        }
        set
        {
            _inst = value;
        }
    }
    [SerializeField] List<I_Unit> units; 
    private Dictionary<HexCoord, int> reachableTiles;
    public List<I_Unit> Units { get { return units; } }
    I_Unit selectedUnit;
    bool playerTurn = true;
    public bool uILock = false;
    [SerializeField] GraphicRaycaster cameraRaycast;
    private I_Unit currentAttacker;
    private I_Unit currentTarget;
    private Quaternion attackerTargetRotation;
    private Quaternion targetTargetRotation;
    [SerializeField] public float animateSpeed = 10f;
    private MovementSequenceState currentMovementState = MovementSequenceState.None;
    private I_Unit movingUnit;
    private List<HexCoord> currentPath;
    
    private FireSequenceState currentFireState = FireSequenceState.None;

    private void Start() {
        cameraRaycast = FindFirstObjectByType<GraphicRaycaster>();
        selectedUnit = null;
        units = new();
        CameraManager.inst.basicControls.Selection.LTap.performed += MouseTap;
    }

    public void StartBattle() {
        foreach (GameObject unitGO in ItemManager.inst.playerUnits)
        {
            I_Unit unit = unitGO.GetComponent<I_Unit>();
            unit.hexPosition=TileManager.inst.playerDeploy[^1];
            TileManager.inst.playerDeploy.RemoveAt(TileManager.inst.playerDeploy.Count-1);
            unit.Init();
        }
        foreach (I_Unit unit in units)
        {
            unit.NOSTankEmpty = false;
            unit.roidsEmpty = false;
            unit.smiteUsed = false;
        }
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
            if (unit.NOSTankUsed)
            {
                unit.actionPoints = 0;
                unit.NOSTankUsed = false;
            }
            else
            {
                unit.actionPoints = unit.maxActionPoints;
            }
            unit.firePoints = unit.maxFirePoints;
            unit.hasBeenHit = false;
            if (unit.reloaderPresent)
            {
                if (Random.value < 0.5f)
                {
                    unit.firePoints++;
                }
            }
        }
    }

    private IEnumerator delayTurnSwitch()
    {
        yield return new WaitForSeconds(0.5f);
        AIManager.inst.OnTurnStart();
    }

    public void EndTurn() {
        EndTurn(false);
    }

    public void EndTurn(bool certain) {
        if(uILock) {
            return;
        }
        if(certain) {
            foreach (I_Unit unit in Units) {
                if (unit.repairmanPresent) {
                    unit.health += 5f;
                    if (unit.health > unit.maxHealth) {
                        unit.health = unit.maxHealth;
                    }
                }
            }
            foreach (I_Unit unit in units){
                unit.OnTurnEnd();
            }
            playerTurn=false;
            SelectUnit(null);
            AIManager.inst.ClearTargets();
            UIManager.inst.SetCertantyDisplay(false);
            StartCoroutine(delayTurnSwitch());
            // AIManager.inst.OnTurnStart();
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
        if(!unit) {
            AIManager.inst.ClearTargets();
            TileManager.inst.HighlightTiles(null);
            return;
        }
        unit.isSelected=true;
        if(unit.actionPoints>0) {
            reachableTiles = Pathfinding.FindReachableTiles(unit.hexPosition, unit.actionPoints,unit,unit.playerControlled);
            TileManager.inst.HighlightTiles(reachableTiles.Keys.ToList());
        } else {
            TileManager.inst.HighlightTiles(null);
        }
    }
    
    void Update()
    {
        if (currentMovementState == MovementSequenceState.Moving)
        {
            return;
        }
        
        switch (currentFireState)
        {
            case FireSequenceState.None:
                break;

            case FireSequenceState.AttackerRotating:
                if (RotateUnitTowards(currentAttacker, attackerTargetRotation))
                {
                    if (!currentTarget.hasBeenHit)
                    {
                        currentFireState = FireSequenceState.TargetRotating;
                    }
                    else
                    {
                        currentFireState = FireSequenceState.Firing;
                    }
                }
                break;

            case FireSequenceState.TargetRotating:
                if (RotateUnitTowards(currentTarget, targetTargetRotation))
                {
                    currentFireState = FireSequenceState.Firing;
                }
                break;

            case FireSequenceState.Firing:
                currentAttacker.Fire(currentTarget);
                currentAttacker.firePoints--;
                currentFireState = FireSequenceState.None;
                currentAttacker = null;
                currentTarget = null;
                uILock = false;
                break;
        }
    }

    public bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new(EventSystem.current)
        {
            position = UnityEngine.Input.mousePosition
        };
        List<RaycastResult> results = new();
        try{
            cameraRaycast.Raycast(eventDataCurrentPosition, results);
        } catch {
            return false;
        }
        // Debug.Log(results.Count);
        // This might result in issues later
        return results.Count > 0;
    }
    
    public void StartFireSequence(I_Unit attacker, I_Unit target)
    {
        if (currentFireState != FireSequenceState.None)
            return;

        currentAttacker = attacker;
        currentTarget   = target;
        Vector3 dirA = target.transform.position - attacker.transform.position;
        dirA.y = 0;
        attackerTargetRotation = Quaternion.LookRotation(dirA, Vector3.up);

        if (!target.hasBeenHit)
        {
            Vector3 dirT = attacker.transform.position - target.transform.position;
            dirT.y = 0;
            targetTargetRotation = Quaternion.LookRotation(dirT, Vector3.up);
        }
        else
        {
            targetTargetRotation = target.model.transform.rotation;
        }

        currentFireState = FireSequenceState.AttackerRotating;
        uILock = true;
    }
    
    private bool RotateUnitTowards(I_Unit unit, Quaternion destRotation)
    {
        Quaternion current = unit.model.transform.rotation;
        Quaternion next = Quaternion.Slerp(current, destRotation, animateSpeed * Time.deltaTime);
        unit.model.transform.rotation = next;

        float angleDiff = Quaternion.Angle(next, destRotation);
        if (angleDiff < 0.1f)
        {
            unit.model.transform.rotation = destRotation;
            return true;
        }
        return false;
    }

    void MouseTap(InputAction.CallbackContext context) {
        if (playerTurn &&  !uILock) { 
            RaycastHit hit; 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            bool hasHit = Physics.Raycast (ray,out hit, 100.0f);
            if (hasHit) { 
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) { 
                    WikiReference wikiRef = hit.transform?.gameObject.GetComponent<WikiReference>();
                    if (wikiRef != null)
                    {
                        UIManager.inst.SetWiki(true);
                        UIManager.inst.ShowWikiPage(wikiRef.wikiID);
                        return;
                    }
                }
                I_Unit unit;
                bool test = false;
                if(IsPointerOverUIObject()) {
                    // UI interactions
                } else if(
                    hit.transform!=null 
                    && (unit = hit.transform.gameObject.GetComponent<I_Unit>())
                    || (unit = UnitAt(HexCoord.AtPosition(new(hit.point.x,hit.point.z))))
                ) { 
                    UIManager.inst.SetDisplayUnit(unit);
                    if(selectedUnit != null && !unit.playerControlled && AIManager.inst.FindTargets(selectedUnit,true).Contains(unit) && selectedUnit.firePoints>0) {
                        StartFireSequence(selectedUnit, unit);
                        SelectUnit(null);
                        
                    } else if(unit.playerControlled && (unit.actionPoints>0 || unit.firePoints>0)) {
                        SelectUnit(unit);
                    }
                } else if(selectedUnit && selectedUnit.playerControlled) {
                    reachableTiles = Pathfinding.FindReachableTiles(selectedUnit.hexPosition, selectedUnit.actionPoints, selectedUnit,true);
                    HexCoord clickedCoord = HexCoord.AtPosition(new(hit.point.x,hit.point.z));
                    if (reachableTiles != null && reachableTiles.ContainsKey(clickedCoord))
                    {
                        MoveUnit(selectedUnit, clickedCoord, true);
                    }
                    else
                    {
                        SelectUnit(null);
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

    public void CheckGameOver() {
        if(units.Count==0) {
            uILock=true;
            SaveManager.inst.DeletePlayerSave(SaveManager.inst.globalSave.currentSave);
            UIManager.inst.GameOver();
        }
    }

    public void MoveUnit(I_Unit unit, HexCoord dest, bool playerTeam) {
        List<HexCoord> path = Pathfinding.FindPath(unit.hexPosition, dest, playerTeam);
        if (path == null || path.Count < 1)  {
            // no valid path or already at destination
            return;
        }

        path.Insert(0,unit.hexPosition);
        
        var splineMover = unit.GetComponent<SplineMovement>();
        if (splineMover == null)  {
            splineMover = unit.gameObject.AddComponent<SplineMovement>();
        }
        
        splineMover.onMovementComplete = () => {
            unit.actionPoints -= reachableTiles[dest]; 
            unit.Move(dest);
            List<HexCoord> tiles = Pathfinding.FindReachableTiles(dest,unit.actionPoints,unit,unit.playerControlled).Keys.ToList();
            if(tiles.Count>1) {
                TileManager.inst.HighlightTiles(tiles);
            } else {
                TileManager.inst.HighlightTiles(null);
            }
            inst.uILock = false;
            currentMovementState = MovementSequenceState.None;
            splineMover.onMovementComplete = () => {;};
        };
        
        movingUnit = unit;
        currentMovementState = MovementSequenceState.Moving;
        uILock = true;

        splineMover.BeginSplineMovement(path);
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