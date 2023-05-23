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
     * 记录玩家死亡时候应该被取消的组件
     */
    [SerializeField] 
    private Behaviour[] componentToDisable;

    private bool[] componentsEnabled;

    private bool colliderEnabled;
    /*
     * 自动帮助我们进行同步所有端口的值
     * 只能在服务端修改，值永远和服务器端的值相同
     * 每次修改之后，会自动将所有的信息同步到所有的客户端
     */
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>();
    public void Setup()
    {
        componentsEnabled = new bool[componentToDisable.Length];
        for (int i = 0; i < componentToDisable.Length; ++i)
        {
            componentsEnabled[i] = componentToDisable[i].enabled;
        }

        Collider col = GetComponent<Collider>();
        colliderEnabled = col.enabled;
        SetDefault();
    }

    private void SetDefault()
    {
        for (int i = 0; i < componentToDisable.Length; ++i)
        {
            componentToDisable[i].enabled = componentsEnabled[i];
        }

        Collider col = GetComponent<Collider>();
        col.enabled = colliderEnabled;
        if (IsServer)
        {
            //这次修改之后，所有的值都会发送到客户端
            currentHealth.Value = maxHealth;
            isDead.Value = false;
        }
    }

    /*
     * 受到伤害
     * 因为在客户端上修改变量之后，服务端上并不会做任何更改，没有意义
     * 在这里我们使得只让服务器端调用这个函数
     */
    public void TakeDamage(int damage)
    {
        if (isDead.Value)
        {
            return;
        }
        currentHealth.Value -= damage;
        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            isDead.Value = true;
            DieOnServer();
            DieClientRpc();
        }
    }

    //重生
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(3f);//停止3秒之后进行重生的逻辑
        
        SetDefault();
        if (IsLocalPlayer)
        {
            transform.position = new Vector3(0f, 3f, 0f);
        }
            
    }

    private void DieOnServer()
    {
        Die();
    }

    /*
     * CLientRpc函数只会在客户端上执行，不会再服务器上执行
     */
    [ClientRpc]
    private void DieClientRpc()
    {
        Die();
    }
    private void Die()
    {
        for (int i = 0; i < componentToDisable.Length; ++i)
        {
            componentToDisable[i].enabled = false;
        }

        Collider col = GetComponent<Collider>();
        col.enabled = false;

        //开启一个新的线程执行重生
        StartCoroutine(Respawn());
    }
    
    public int GetHealth()
    {
        return currentHealth.Value;
    }
}
