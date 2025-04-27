using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Stack : BasicUnit
{
    public override void Fire(I_Unit target)
    {
        base.Fire(target);
        base.Fire(target);
        base.Fire(target);
    }
}