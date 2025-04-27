using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WikiButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(SoundManager.inst.PlayBasicButton);
        GetComponent<Button>().onClick.AddListener(() => UIManager.inst.ShowWikiPage(name.Split("-")[0]));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
