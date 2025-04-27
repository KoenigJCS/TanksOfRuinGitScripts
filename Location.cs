using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;


public class Location : MonoBehaviour
{
    public UnityEngine.UI.Image stateIcon;
    public List<Location> nextLocations = new();
    public Button button;
    public int state = 0;
    public int uniqueID;
    public int order;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
