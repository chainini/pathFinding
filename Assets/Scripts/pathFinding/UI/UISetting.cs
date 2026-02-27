using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISetting : UIView
{
    public Button btnClose;
    
    private void OnEnable()
    {
        btnClose.onClick.AddListener(()=> UIManager.Instance.CloseUI(this));
    }

    private void OnDisable()
    {
        btnClose.onClick.RemoveAllListeners();
    }
    
    public override void OnOpen()
    {
        
        
        Debug.Log("OnOpen UISetting");
    }

    public override void OnClose()
    {
        
    }
}
