using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] 
    private Button hostBtn;

    [SerializeField] 
    private Button serverBtn;

    [SerializeField] 
    private Button clientBtn;
    // Start is called before the first frame update
    void Start()
    {
        //添加按钮的监听事件
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        serverBtn.onClick.AddListener((() =>
        {
            NetworkManager.Singleton.StartServer();
        }));
        clientBtn.onClick.AddListener((() =>
        {
            NetworkManager.Singleton.StartClient();
        }));
        
    }


}