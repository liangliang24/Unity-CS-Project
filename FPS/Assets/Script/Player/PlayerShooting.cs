using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";
    private WeaponManager weaponManager;
    private PlayerWeapon currentWeapon;

    private float shootCoolDownTime = 1f;//距离上次开枪过了多少秒
    private int autoShootCount = 0;//当前一共连开多少枪
    [SerializeField] private LayerMask mask;
    [SerializeField] private PlayerController playerController;
    private Camera cam;

    enum HitEffectMaterial
    {
        Metal,
        Stone
    }
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        weaponManager = GetComponent<WeaponManager>();
        playerController = GetComponent<PlayerController>();
    }

    
    // Update is called once per frame
    void Update()
    {
        
        
        if (!IsLocalPlayer)
        {
            return;
        }
        currentWeapon = weaponManager.GetCurrentWeapon();
        if (currentWeapon.shootRate<=0)
        {
            autoShootCount = 0;
            shootCoolDownTime += Time.deltaTime;
        }
        if (currentWeapon.shootRate <= 0)
        {
            //这里判断单次点击，实现单发模式，连发模式需要用到事件
            if (Input.GetButtonDown("Fire1")&&shootCoolDownTime>=currentWeapon.shootCoolDownTime)
            {
                autoShootCount = 0;
                Shoot();
                shootCoolDownTime = 0f;
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                //通过周期性地调用一个函数实现连发
                InvokeRepeating("Shoot",0f,1f/currentWeapon.shootRate);
            }
            else if(Input.GetButtonUp("Fire1")||Input.GetKey(KeyCode.Q))
            {
                CancelInvoke("Shoot");
            }
        }
        
    }

    /*
     * 由于OnShoot的渲染需要在所有玩家上进行，
     * 那么PlayerSetup中需要取消禁用非本地PlayerShooting的功能
     * 然后进行特判即可
     */
    private void OnShoot(float recoilForce)
    {
        weaponManager.GetCurrentGraphics().muzzelFlash.Play();
        weaponManager.getCurrentAudio().Play();

        //施加后坐力
        if (IsLocalPlayer)
        {
            playerController.AddRecoilForce(recoilForce);
            // weaponHolder.transform.Rotate(new Vector3(10f,0f,0f));
        }
    }

    [ServerRpc]
    private void OnShootServerRpc(float recoilForce)
    {
        if (!IsHost)
        {
            OnShoot(recoilForce);
        }
        
        OnShootClientRpc(recoilForce);
    }

    [ClientRpc]
    private void OnShootClientRpc(float recoilForce)
    {
        OnShoot(recoilForce);
    }


    //击中点特效
    private void OnHit(Vector3 pos, Vector3 normal,HitEffectMaterial material)
    {
        GameObject hitEffectPrefab;
        if (material == HitEffectMaterial.Metal)
        {
            hitEffectPrefab = weaponManager.GetCurrentGraphics().metalHitEffectPrefab;
        }
        else
        {
            hitEffectPrefab = weaponManager.GetCurrentGraphics().stoneHitEffectPrefab;
        }

        GameObject hitEffectObject = Instantiate(hitEffectPrefab, pos, Quaternion.LookRotation(normal));

        ParticleSystem particleSystem = hitEffectObject.GetComponent<ParticleSystem>();
        particleSystem.Emit(1);
        Destroy(hitEffectObject,1f);
    }

    [ServerRpc]
    private void OnHitServerRpc(Vector3 pos, Vector3 normal,HitEffectMaterial material)
    {
        if (!IsHost)
        {
            OnHit(pos,normal,material);
        }
        OnHitClientRpc(pos,normal,material);
    }

    [ClientRpc]
    private void OnHitClientRpc(Vector3 pos, Vector3 normal,HitEffectMaterial material)
    {
        OnHit(pos,normal,material);
    }
    /*
     * 这里射击的逻辑是
     * 从眼睛射出一条射线，如果这条射线击中了谁，那么那个人就是被击中的结果.
     * 这里的射击还需要联网，用到ServerRPC的功能。让客户端去调用服务端的函数。
     * 因为只允许一个人开枪，所以其他人的PlayerShooting组件应该像之前那样在Setup中禁用掉
     */
    private void Shoot()
    {
        autoShootCount++;
        float recoilFoce = currentWeapon.recoilForce;
        if (autoShootCount<=3)
        {
            recoilFoce /= 5;
        }
        OnShootServerRpc(recoilFoce);
        
        // Debug.Log("Shoot!!!");
        RaycastHit hit;
        /*
         * mask是判断求交什么物体，如果这个物体不在所选的layer内，那么不会产生射击。
         * 这个可以看作是友军伤害的方法。
         */
        if (Physics.Raycast(cam.transform.position,
                cam.transform.forward,
                out hit,
                currentWeapon.range,mask))
        {
            if (hit.collider.tag == PLAYER_TAG)
            {
                ShootServerRpc(hit.collider.name,currentWeapon.damage);
                OnHitServerRpc(hit.point,hit.normal,HitEffectMaterial.Metal);
            }
            else
            {
                OnHitServerRpc(hit.point,hit.normal,HitEffectMaterial.Stone);                
            }
            
        }
    }

    
    
    
    /*
     * 如果一个玩家进行了射击，那么就会调用服务器上的ShootServerRpc函数
     */
    [ServerRpc]
    private void ShootServerRpc(string hitteName,int damage)
    {
        Player play = GameManager.Singleton.GetPlyaer(hitteName);
        play.TakeDamage(damage);
    }
}
