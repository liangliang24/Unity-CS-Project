using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]//使得编辑器可以调整这个值
    private float speed = 5f;//角色移动的速度

    [SerializeField] 
    private PlayerController controller;

    [SerializeField] 
    private float lookSensitivity = 8f;//横向鼠标灵敏度

    [SerializeField] 
    private float thrusterForce = 20f;//飞行的推力

    [SerializeField] 
    private ConfigurableJoint joint;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;//在游戏开始的时候让鼠标消失
    }

    // Update is called once per frame
    void Update()
    {
        float xMov = Input.GetAxisRaw("Horizontal");//获取横向方向的输入
        float yMov = Input.GetAxisRaw("Vertical");//获取纵向方向的输入
        //获取鼠标的移动
        float xMouse = Input.GetAxisRaw("Mouse X");
        float yMouse = Input.GetAxisRaw("Mouse Y");
        
        
        //将两个方向的输入转换为一个向量,得到物体应该运动的方向
        Vector3 velocity = (transform.right * xMov + transform.forward * yMov).normalized;
        Vector3 yRotation = new Vector3(0f, xMouse, 0f)*lookSensitivity;
        Vector3 xRotation = new Vector3(-yMouse, 0f, 0f)*lookSensitivity;
        
        //获取关于飞行的输入
        Vector3 force = Vector3.zero;
        //如果接收到空格的输入那么取消弹力，如果没有那么恢复弹力
        if (Input.GetButton("Jump"))
        {
            force = Vector3.up * thrusterForce;
            var jointYDrive = joint.yDrive;
            jointYDrive.positionSpring = 0f;
            jointYDrive.positionDamper = 0f;
            jointYDrive.maximumForce = 0f;
        }
        else
        {
            force = Vector3.zero;
            var jointYDrive = joint.yDrive;
            jointYDrive.positionSpring = 20f;
            jointYDrive.positionDamper = 0f;
            jointYDrive.maximumForce = 40f;
        }
            
            
        controller.Move(velocity*speed);
        controller.Rotate(yRotation,xRotation);
        controller.Thrust(force);
        
    }
}
