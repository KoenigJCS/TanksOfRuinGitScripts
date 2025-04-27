using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Rammer : BasicUnit
{
    public override void Fire(I_Unit target)
    {
        base.Fire(target);
        health-=10f;
        SetHealthBarVis(true);
        visHealthBarTimer=2.5f;
    }
}