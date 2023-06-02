using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/*
 * 这个类也是需要网络通信的，因此需要使用到NetworkManger中的功能
 */
public class WeaponManager : NetworkBehaviour
{
    //玩家的主武器
    [SerializeField] private PlayerWeapon primaryWeapon;
    
    [SerializeField] private PlayerWeapon secondaryWeapon;
    
    [SerializeField] private GameObject weaponHolder;
    //玩家当前的武器
    private PlayerWeapon currentWeapon;
    
    //当前攻击的特效
    private WeaponGraphics currentGraphics;
    
    //音频效果
    private AudioSource currenAudio;
    // Start is called before the first frame update
    void Start()
    {
        EquipWeapon(primaryWeapon);
        
    }
    

    private GameObject weaponObject;
    public void EquipWeapon(PlayerWeapon weapon)
    {
        currentWeapon = weapon;        

        //在装备武器的时候先卸掉当前的武器
        while (weaponHolder.transform.childCount>0)
        {
            DestroyImmediate(weaponHolder.transform.GetChild(0).gameObject);
        }
        
        // Instantiate可以实例化一个对象
        weaponObject = Instantiate(
            currentWeapon.graphics,
            weaponHolder.transform.position,
            weaponHolder.transform.rotation);
        weaponObject.transform.SetParent(weaponHolder.transform);

        currentGraphics = weaponObject.GetComponent<WeaponGraphics>();
        currenAudio = weaponObject.GetComponent<AudioSource>();
        //对于自己开枪的音效对自己来说是2D的，如果是3D的音效会容易使得玩家眩晕
        if (IsLocalPlayer)
        {
            currenAudio.spatialBlend = 0f;
        }

    }

    public void Recoil(float recoilForce)
    {
        weaponHolder.transform.GetChild(0).gameObject.transform.Rotate(new Vector3(0f, 0f, recoilForce*5));
    }
    public AudioSource getCurrentAudio()
    {
        return currenAudio;
    }
    
    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }

    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    //切换武器
    public void ToggleWeapon()
    {
        if (currentWeapon==primaryWeapon)
        {
            EquipWeapon(secondaryWeapon);
        }
        else
        {
            EquipWeapon(primaryWeapon);
        }
    }

    [ClientRpc]
    private void ToggleWeaponClientRpc()
    {
        ToggleWeapon();
    }
    [ServerRpc]
    private void ToggleWeaponServerRpc()
    {
        if (!IsHost)
        {
            ToggleWeapon();
        }
        
        ToggleWeaponClientRpc();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (IsLocalPlayer)
            {
                ToggleWeaponServerRpc();
            }
        }
    }
    
        
}
