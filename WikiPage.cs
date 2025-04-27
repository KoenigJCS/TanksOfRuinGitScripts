using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WikiPage : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIManager.inst.AddWiki(gameObject);
        gameObject.SetActive(false);
    }
}
