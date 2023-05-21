using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static string info;

    public static void UpadateInfo(string _info)
    {
        info = _info;
    }
    
    //每一帧至少调用一次，在渲染的时候在屏幕上画一些东西
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(200f,200f,200f,400f));
        GUILayout.BeginVertical();
        
        
        GUILayout.Label(info);
        
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
