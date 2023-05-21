using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private PlayerWeapon weapon;
    [SerializeField] private LayerMask mask;
    
    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    
    // Update is called once per frame
    void Update()
    {
        //这里判断单次点击，实现单发模式，连发模式需要用到事件
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    /*
     * 这里射击的逻辑是
     * 从眼睛射出一条射线，如果这条射线击中了谁，那么那个人就是被击中的结果.
     * 这里的射击还需要联网，用到ServerRPC的功能。让客户端去调用服务端的函数。
     * 因为只允许一个人开枪，所以其他人的PlayerShooting组件应该像之前那样在Setup中禁用掉
     */
    
    private void Shoot()
    {
        RaycastHit hit;
        /*
         * mask是判断求交什么物体，如果这个物体不在所选的layer内，那么不会产生射击。
         * 这个可以看作是友军伤害的方法。
         */
        if (Physics.Raycast(cam.transform.position,
                cam.transform.forward,
                out hit,
                weapon.range,mask))
        {
            ShootServerRpc(hit.collider.name);
        }
    }
    /*
     * 如果一个玩家进行了射击，那么就会调用服务器上的ShootServerRpc函数
     */
    [ServerRpc]
    private void ShootServerRpc(string hitteName)
    {
        GameManager.UpadateInfo(transform.name+" hit "+hitteName);
    }
}