using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;
    
    //为了清楚名字和玩家的对应关系，这里通过一个字典去进行管理
    private static Dictionary<string, Player> players = new Dictionary<string, Player>();

    private void Awake()
    {
        Singleton = this;
    }

    public void RegisterPlayer(string name, Player player)
    {
        player.transform.name = name;
        players.Add(name,player);
        Singleton.GetPlyaer(name).Setup();
    }

    public void UnregisterPlayer(string name)
    {
        players.Remove(name);
    }
    //每一帧至少调用一次，在渲染的时候在屏幕上画一些东西
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(200f,200f,200f,400f));
        GUILayout.BeginVertical();

        GUI.color = Color.red;
        foreach (string name in players.Keys)
        {
            
            GUILayout.Label(name + ":" + GetPlyaer(name).GetHealth());
        }
        
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    public Player GetPlyaer(string name)
    {
        return players[name];
    }
}
