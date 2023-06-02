 using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
 using Unity.Mathematics;
 using Unity.Netcode;
 using UnityEngine;
using UnityEngine.PlayerLoop;
 using UnityEngine.SocialPlatforms;
 using FixedUpdate = Unity.VisualScripting.FixedUpdate;
 using Random = System.Random;
 using Vector3 = UnityEngine.Vector3;

public class PlayerController : NetworkBehaviour
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

    //记录上一帧的位置
    private Vector3 lastFramePosition = Vector3.zero;
    //精度误差，
    private float eps = 0.01f;
    //动画
    private Animator animator;

    private float distToGround;
    private void Start()
    {
        lastFramePosition = transform.position;
        animator = GetComponentInChildren<Animator>();
        distToGround = GetComponent<Collider>().bounds.extents.y;
    }

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
            thrusterForce = Vector3.zero;
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
    
    
    
    
    /*
     * 动画中的参数direction代表了8个方向，每次根据变化的方向来判断出角色走的方向
     * 然后可以播放相应的动画
     */
    public void PerformAnimation()
    {
        if (animator.GetInteger("direction") == -1)
        {
            return;
        }

        if (!Physics.Raycast(transform.position,-Vector3.up,distToGround+0.1f))
        {
            animator.SetInteger("direction",9);
            return;
        }
        Vector3 deltaPosition = transform.position - lastFramePosition;
        lastFramePosition = transform.position;

        float forward = Vector3.Dot(deltaPosition, transform.forward);
        float right = Vector3.Dot(deltaPosition, transform.right);

        int direction = 0;

        if (forward > eps)
        {
            direction = 1;//向前
        }
        else if(forward<-eps)
        {
            if (right > eps)
            {
                direction = 4;//右后
            }
            else if(right < -eps)
            {
                direction = 6; //左后
            }
            else
            {
                direction = 5;//后
            }
        }
        else
        {
            if (right > eps)
            {
                direction = 3;
            }
            else if(right < -eps)
            {
                direction = 7;
            }
        }
        
        animator.SetInteger("direction",direction);
    }
    //物体的模拟通过Update，但是如果物体的模拟是刚体用的是FixedUpdate
    /*FixedUpdate每秒默认执行50次（可以自行设置,Edit-Project Settings-Time中可以设置），这个是均匀的执行过程，每秒钟严格均匀地执行。
        Update每秒执行次数也是定值（两者并不一样），但是每次执行的时间并不一定均匀。
        这会导致一个问题，如果模拟的是一个曲线的时候，因为时间并不均匀，导致整个模拟和真实模拟的轨迹并不一致
        */
    private void FixedUpdate()
    {
        //只有本地玩家才能操作自己的位置，但是所有人都可以播放对应的动画
        //实现动画的联机
        if (IsLocalPlayer)
        {
            PerformMovement();
            PerformRotation();
        }
        
        /*
         * 因为本地采用fixedupdate，远程采用update，
         * 如果统一在update更新动画，两者不同步有可能会导致滑步的出现
         */
        if (IsLocalPlayer)
        {
            PerformAnimation();
        }
        
        // weaponManager.Recoil();
    }

    /*
     * 由于网路同步的频率不一定和FixUpdate一样严格
     * 那么FixUpdate的调用可能比网路同步的频率多，导致最后出现动画没有达到预期的效果
     */
    private void Update()
    {
        if (!IsLocalPlayer)
        {
            PerformAnimation();
        }
        
    }
}
