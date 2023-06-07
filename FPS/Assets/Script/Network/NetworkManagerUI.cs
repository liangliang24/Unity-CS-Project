using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using NetworkTransport = Unity.Netcode.NetworkTransport;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button refreshBtn;
    [SerializeField] private Button buildBtn;
    
    [SerializeField] 
    private Button hostBtn;

    [SerializeField] 
    private Button serverBtn;

    [SerializeField] 
    private Canvas MenuUI;
    //用于加入房间的按钮，在刷新之后更新
    [SerializeField] 
    private GameObject roomButtonPrefab;
    [SerializeField] 
    private Button clientBtn;

    [SerializeField] private Button Room1;

    [SerializeField] private Button Room2;

    private int buildRoomPort = -1;
    //存储当前所有的房间
    private List<Button> rooms = new List<Button>();
    // Start is called before the first frame update
    void Start()
    {
        SetConfig();
        
        InitButtons();
        
        RefreshRoomList();
    }

    //添加事件监听，当玩家关闭窗口的时候，删除这个房间
    private void OnApplicationQuit()
    {
        RemoveRoom();
    }

    
    private void SetConfig()
    {
        //unet transport的connet address要改成我们服务器的公网IP
        //可以用于接收命令行
        var args = System.Environment.GetCommandLineArgs();

        //接收端口号
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-port")
            {
                int port = int.Parse(args[i + 1]);
                var transport = GetComponent<UNetTransport>();
                transport.ConnectPort = transport.ServerListenPort = port;
            }
        }
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-lauch-as-server")
            {
                NetworkManager.Singleton.StartServer();
                DestroyAllButtons();
            }
        }
    }
    //添加按钮的监听事件
    private void InitButtons()
    {
        refreshBtn.onClick.AddListener(() =>
        {
            RefreshRoomList();
        });
        buildBtn.onClick.AddListener(() =>
        {
            BuildRoomRequest();
        });
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            DestroyAllButtons();
        });
        serverBtn.onClick.AddListener((() =>
        {
            NetworkManager.Singleton.StartServer();
            DestroyAllButtons();
        }));
        clientBtn.onClick.AddListener((() =>
        {
            NetworkManager.Singleton.StartClient();
            DestroyAllButtons();
        }));
    }
    //unity获取https请求
    //获取房间
    private void RefreshRoomList()
    {
        StartCoroutine(RefreshRoomListRequest("http://8.130.131.43:8080/fps/get_room_list/"));
    }

    IEnumerator RefreshRoomListRequest(string uri)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.ConnectionError)
        {
            var resp = JsonUtility.FromJson<GetRoomList>(uwr.downloadHandler.text);

            //删除掉所有已有的按钮
            foreach (var room in rooms)
            {
                room.onClick.RemoveAllListeners();
                Destroy(room.gameObject);
            }
            rooms.Clear();
            print(resp.error_message);

            int k = 0;
            foreach (var room in resp.rooms)
            {
                GameObject buttonObj = Instantiate(roomButtonPrefab, MenuUI.transform);
                buttonObj.transform.localPosition = new Vector3(-21, 92 - k * 60);
                Button button = buttonObj.GetComponent<Button>();
                button.GetComponentInChildren<TextMeshProUGUI>().text = room.name;
                button.onClick.AddListener(() =>
                {
                    var transport = GetComponent<UNetTransport>();
                    transport.ConnectPort = transport.ServerListenPort = room.port;
                    NetworkManager.Singleton.StartClient();
                    DestroyAllButtons();    
                });
                ++k;
                rooms.Add(button);
            }
        }
    }
    //创建房间
    private void BuildRoomRequest()
    {
        StartCoroutine(BuildRoomListRequest("http://8.130.131.43:8080/fps/build_room/"));
    }

    IEnumerator BuildRoomListRequest(string uri)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.ConnectionError)
        {
            var resp = JsonUtility.FromJson<BuildRoom>(uwr.downloadHandler.text);
            print(resp.error_message);
            if (resp.error_message == "success")
            {
                var transport = GetComponent<UNetTransport>();

                transport.ConnectPort = transport.ServerListenPort = buildRoomPort = resp.port;

                NetworkManager.Singleton.StartClient();
                DestroyAllButtons();
            }
        }
    }
    private void RemoveRoom()
    {
        StartCoroutine(RemoveRoomRequest("http://8.130.131.43:8080/fps/remove_room/?port=" +
                                         buildRoomPort));
    }
    IEnumerator RemoveRoomRequest(string uri)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.ConnectionError)
        {
            var resp = JsonUtility.FromJson<RemoveRoom>(uwr.downloadHandler.text);
            if (resp.error_message == "success")
            {
                
            }
        }
    }
    private void DestroyAllButtons()
    {
        refreshBtn.onClick.RemoveAllListeners();
        buildBtn.onClick.RemoveAllListeners();
        Destroy(refreshBtn.gameObject);
        Destroy(buildBtn.gameObject);
        Destroy(hostBtn.gameObject);
        Destroy(serverBtn.gameObject);
        Destroy(clientBtn.gameObject);
        foreach (var room in rooms)
        {
            room.onClick.RemoveAllListeners();
            Destroy(room.gameObject);
        }
    }
}
