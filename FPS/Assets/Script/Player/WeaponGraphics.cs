using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [Serializable]
public class WeaponGraphics : MonoBehaviour
{
    //开枪的特效
    public ParticleSystem muzzelFlash;
    
    //击中金属物体的特效
    public GameObject metalHitEffectPrefab;
    
    //击中石头的特效
    public GameObject stoneHitEffectPrefab;

}
