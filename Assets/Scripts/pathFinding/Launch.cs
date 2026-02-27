using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.OpenUI(UIID.RPGHUD);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
