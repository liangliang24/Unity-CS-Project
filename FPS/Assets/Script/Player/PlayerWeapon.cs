using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//使得能够串行化
[Serializable]
public class PlayerWeapon
{
    public string name = "M16A1";
    public int damage = 10;
    public float range = 100f;//射击距离
}
