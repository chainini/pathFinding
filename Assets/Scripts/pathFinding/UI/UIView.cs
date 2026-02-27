using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIView : MonoBehaviour
{
    public UIID uiID;
    
    public UIType uiType;
    
    public UIAnimType uiAnimType;

    public bool isFullInput;

    public bool isDestroy;

    public abstract void OnOpen();


    public abstract void OnClose();

    public virtual void OnTop()
    {
        
    }
    

    public virtual void CustomOpenAnim()
    {
        
    }

    public virtual void CustomCloseAnim()
    {
        
    }
}
