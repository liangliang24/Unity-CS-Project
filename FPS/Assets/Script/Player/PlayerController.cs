 using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
 using Unity.Mathematics;
 using UnityEngine;
using UnityEngine.PlayerLoop;
using FixedUpdate = Unity.VisualScripting.FixedUpdate;
 using Random = System.Random;
 using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    //获取玩家的物理属性
    [SerializeField] 
    private Rigidbody rb;

    [SerializeField] 
    private Camera cam;
    
    private Vector3 velocity = Vector3.zero;//每秒钟移动的距离
    private Vector3 yRotation = Vector3.zero;//旋转角色
    private Vector3 xRotation = Vector3.zero;//旋转视角
    private Vector3 thrusterForce = Vector3.zero;//向上的推力
    [SerializeField] private WeaponManager weaponManager;
    //后坐力
    private float recoilForce = 2f;
    //这两个参数限制相机的旋转
    [SerializeField] 
    private float xRotationLimit = 85f;
    
    private float xRotationTotal = 0f;

    public void AddRecoilForce(float newRecoilForce)
    {
        recoilForce += newRecoilForce;
    }
    //获取移动和旋转
    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void Rotate(Vector3 _yRotation, Vector3 _xRotation)
    {
        yRotation = _yRotation;
        xRotation = _xRotation;
    }

    public void Thrust(Vector3 _thrusterForce)
    {
        thrusterForce = _thrusterForce;
    }
    //朝着velocity方向进行移动
    private void PerformMovement()
    {
        if (recoilForce < 1)
        {
            recoilForce = 0f;
        }
        
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position+velocity*Time.fixedDeltaTime);//模拟物体下一帧的位置，x+vt
        }
        //给刚体作用一个向上的力
        if (thrusterForce != Vector3.zero)
        {
            rb.AddForce(thrusterForce);//作用Time.fixedDeltaTime秒
        }
    }
    
    //进行旋转
    private void PerformRotation()
    {
        if (yRotation != Vector3.zero||recoilForce>=0)
        {
            rb.transform.Rotate(yRotation+rb.transform.up*new Random().Next(-(int)recoilForce,(int)recoilForce));
        }

        if (xRotation != Vector3.zero||recoilForce>=0)
        {
            xRotationTotal += xRotation.x-recoilForce;
            xRotationTotal = Mathf.Clamp(xRotationTotal, -xRotationLimit,xRotationLimit);
            cam.transform.localEulerAngles = new Vector3(xRotationTotal,0f, 0f);
        }
        
        
        //实现先快后慢的效果
        recoilForce *= 0.5f;
    }
    
    
    //物体的模拟通过Update，但是如果物体的模拟是刚体用的是FixedUpdate
    /*FixedUpdate每秒默认执行50次（可以自行设置,Edit-Project Settings-Time中可以设置），这个是均匀的执行过程，每秒钟严格均匀地执行。
        Update每秒执行次数也是定值（两者并不一样），但是每次执行的时间并不一定均匀。
        这会导致一个问题，如果模拟的是一个曲线的时候，因为时间并不均匀，导致整个模拟和真实模拟的轨迹并不一致
        */
            
    private void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
        
        // weaponManager.Recoil();
    }
}
