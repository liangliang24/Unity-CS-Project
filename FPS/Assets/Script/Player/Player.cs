using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/*
 * 每一个Player会在所有的端口执行，在所有端口都会初始化，但是只有服务器端有效
 */
public class Player : NetworkBehaviour
{
    [SerializeField] 
    private int maxHealth = 100;

    /*
     * 自动帮助我们进行同步所有端口的值
     * 只能在服务端修改，值永远和服务器端的值相同
     * 每次修改之后，会自动将所有的信息同步到所有的客户端
     */
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public void Setup()
    {
        SetDefault();
    }

    private void SetDefault()
    {
        if (IsServer)
        {
            //这次修改之后，所有的值都会发送到客户端
            currentHealth.Value = maxHealth;
        }
    }

    /*
     * 受到伤害
     * 因为在客户端上修改变量之后，服务端上并不会做任何更改，没有意义
     * 在这里我们使得只让服务器端调用这个函数
     */
    public void TakeDamage(int damage)
    {
        currentHealth.Value -= damage;
        if (currentHealth.Value < 0)
        {
            currentHealth.Value = 0;
        }
    }

    public int GetHealth()
    {
        return currentHealth.Value;
    }
}
