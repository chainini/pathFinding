using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    private List<GameObject> uiList = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();

        var res = Resources.LoadAll<UIView>("prefab");
        
        Debug.Log(res.Length);
        
        foreach (var o in res)
        {
            uiList.Add(o.gameObject);
        }
    }

    private Stack<UIView> windowViews = new Stack<UIView>();
    
    private Stack<UIView> popupViews = new Stack<UIView>();

    public void OpenUI(UIID uiID)
    {
        var ui = uiList.Find(e => e.GetComponent<UIView>().uiID == uiID);
        if (!ui)
        {
            Debug.LogError("UI not found");
            return;
        }
        
        var uiObject = Instantiate(ui);
        var uiView = uiObject.GetComponent<UIView>();
        var uiType = uiView.uiType;
        var uiAnim = uiView.uiAnimType;
        var parent = this.transform;
        
        uiObject.gameObject.SetActive(false);

        switch (uiType)
        {
            case UIType.HUD:
                parent = FindInScreenUI("HUD");
                break;
            case UIType.Window:
                parent = FindInScreenUI("Window");
                windowViews.Push(uiView);
                break;
            case UIType.Overlay:
                parent = FindInScreenUI("OverLay");
                break;
            case UIType.Popup:
                parent = FindInScreenUI("Popup");
                popupViews.Push(uiView);
                break;
        }
        
        uiObject.transform.SetParent(parent);
        uiObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        switch (uiAnim)
        {
            case UIAnimType.Pop:
                uiObject.gameObject.SetActive(true);
                break;
            case UIAnimType.Fade:
                break;
            case UIAnimType.Zoom:
                break;
            case UIAnimType.Custom:
                break;
        }
        
        uiView.OnOpen();
    }

    public void CloseUI(UIView uiView)
    {
        
        var uiType = uiView.uiType;
        var uiAnim = uiView.uiAnimType;
        var isWho = UIType.Window;
        
        switch (uiType)
        {
            case UIType.HUD:
                uiView.gameObject.SetActive(false);
                uiView.OnClose();
                return;
            case UIType.Overlay:
                uiView.gameObject.SetActive(false);
                uiView.OnClose();
                return;
            case UIType.Window:
                // 验证关闭的是栈顶
                if (windowViews.Count == 0 || windowViews.Peek() != uiView)
                {
                    Debug.LogWarning($"Window {uiView.uiID} is not on top of stack!");
                    return;
                }
                
                windowViews.Pop();
                
                // 如果还有下一个Window，让它重新成为栈顶
                if (windowViews.Count > 0)
                {
                    windowViews.Peek().OnTop();
                }
                break;
            case UIType.Popup:
                // 验证关闭的是栈顶
                if (popupViews.Count == 0 || popupViews.Peek() != uiView)
                {
                    Debug.LogWarning($"Popup {uiView.uiID} is not on top of stack!");
                    return;
                }
                
                popupViews.Pop();
                
                // 如果还有下一个Popup，让它重新成为栈顶
                if (popupViews.Count > 0)
                {
                    popupViews.Peek().OnTop();
                }
                break;
        }
        
        
        
        switch (uiAnim)
        {
            case UIAnimType.Pop:
                uiView.OnClose();
                Destroy(uiView.gameObject);
                break;
            case UIAnimType.Fade:
                break;
            case UIAnimType.Zoom:
                break;
            case UIAnimType.Custom:
                break;
        }
    }
    
    Transform FindInScreenUI(string name)
    {
        Scene uiScene = SceneManager.GetSceneByName("ScreenUI");
        if (!uiScene.isLoaded)
        {
            Debug.LogWarning("ScreenUI scene not loaded.");
            return null;
        }
        
        foreach (var root in uiScene.GetRootGameObjects())
        {
            var t = root.transform.Find(name);
            if (t != null)
                return t;
        }

        return null;
    }
}
