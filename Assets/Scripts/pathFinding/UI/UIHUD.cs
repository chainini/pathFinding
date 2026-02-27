using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHUD : UIView
{
    public Button btnSetting;


    private void OnEnable()
    {
        btnSetting.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenUI(UIID.Setting);
            Debug.Log("OnOpen UISetting");
        });
    }

    private void OnDisable()
    {
        btnSetting.onClick.RemoveAllListeners();
    }

    public override void OnOpen()
    {
        
        
        Debug.Log("OnOpen UIHUD");
    }

    public override void OnClose()
    {
        
    }
}
