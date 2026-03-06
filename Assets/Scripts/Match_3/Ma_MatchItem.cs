using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ma_MatchItem : MonoBehaviour
{
    public Vector2 myPos;
    public Image myImg;
    public int myType;
    public void SetData(Vector2 pos, int type)
    {
        myPos = pos;
        myType = type;
        switch (type)
        {
            case 1:
                myImg.color = Color.red;
                break;
            case 2:
                myImg.color = Color.yellow;
                break;
            case 3:
                myImg.color = Color.green;
                break;
            case 4:
                myImg.color = Color.blue;
                break;
            case 5:
                myImg.color = Color.black;
                break;
            case 0:
                myImg.color = new Color(0.2f, 0.2f, 0.2f, 0);
                break;
        }
    }
}
