using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Collections.Generic;
using Settworks.Hexagons;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(I_Unit))]
public class SplineMovement : MonoBehaviour
{
    private SplineContainer splineContainer;
    private I_Unit unit;
    private bool isMoving = false;
    private float distanceTraveled = 0f;
    private float splineLength = 0f;

    public Action onMovementComplete;

    void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        unit = GetComponent<I_Unit>();
    }

    public void BeginSplineMovement(List<HexCoord> path)
    {
        splineContainer.Splines[0].Clear();

        foreach (HexCoord hex in path)
        {
            Vector3 worldPos = new(hex.Position().x, 1, hex.Position().y);
            BezierKnot knot = new(worldPos, Vector3.forward, Vector3.back, Quaternion.identity);
            
            splineContainer.Splines[0].Add(knot,TangentMode.AutoSmooth);
        }

        splineLength = splineContainer.Splines[0].GetLength();
        distanceTraveled = 0f;

        isMoving = true;
    }

    void Update()
    {
        if (!isMoving || splineLength <= .01f)
            return;

        distanceTraveled += PlayerManager.inst.animateSpeed * Time.deltaTime;

        float t = distanceTraveled / splineLength;

        // Evaluate the position at t
        Vector3 newPos = splineContainer.Splines[0].EvaluatePosition(t);
        transform.position = newPos;
        
        Vector3 nextPos = splineContainer.Splines[0].EvaluatePosition(t+Time.deltaTime);
        Vector3 facing = nextPos - newPos;
        if(t <= 0.95f)
            unit.model.transform.rotation = Quaternion.LookRotation(facing);

        if (t >= 1f) {
            t = 1f;
            isMoving = false;
            onMovementComplete.Invoke();
        }
    }
}
