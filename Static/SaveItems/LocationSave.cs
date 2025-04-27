using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct LocationSave
{
    public List<int> neighborOrder;
    public int locationID;
    public int state;
    public int order;
    public LocationSave(List<int> n_neighborOrder,int n_locationID=-1, int n_state = 0,int n_order = 0) {
        locationID=n_locationID;
        neighborOrder=n_neighborOrder;
        state=n_state;
        order=n_order;
    }
}