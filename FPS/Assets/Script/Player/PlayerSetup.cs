using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//因为这个是判断是否为本地玩家，所以需要引用网络包的API，所以需要继承自NetworkBehaviour
public class PlayerSetup : NetworkBehaviour
{

    [SerializeField] 
    private Behaviour[] componentsToDisable;//需要禁用的组件的列表
    
    private Camera sceneCamera;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //如果当前加入的玩家不是本地玩家，那么将这个玩家的各个组件禁用掉，让它无法读取输入。
        //否则将场景摄像机关闭，知道角色死亡在启用。
        if (!IsLocalPlayer)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
        else
        {
            sceneCamera = Camera.main;//将场景摄像机设置为MainCamera
            if (sceneCamera != null)
            {
                sceneCamera.gameObject.SetActive(false);
            }
        }
        
        RegisterPlayer();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (sceneCamera!=null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
        
        GameManager.Singleton.UnregisterPlayer(transform.name);
    }

    private void RegisterPlayer()
    {
        string name = "Player " + GetComponent<NetworkObject>().NetworkObjectId.ToString();

        Player player = GetComponent<Player>();
        
        GameManager.Singleton.RegisterPlayer(name,player);
    }
    // Update is called once per frame
    private void OnDisable()
    {
        if (sceneCamera!=null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
    }
    
    
}
