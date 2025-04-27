using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    public static CameraManager inst;
    private void Awake() {
        inst = this;
    }
    public BasicControls basicControls;
    [SerializeField] Transform gameCamera;
    [SerializeField] Vector2 cameraTLCorner;
    [SerializeField] Vector2 cameraBRCorner;
    [SerializeField] Vector2 cameraHRange;
    bool dragDown = false;
    public bool holding = false;
    [SerializeField] float moveSenitivity, scrollSensitivity;
    // [SerializeField] Vector3 camStartPos;
    // Start is called before the first frame update
    void Start()
    {
        basicControls = new();
        if(basicControls!=null) {
            basicControls.Enable();
        } else {
            Debug.LogError("No Mouse Actions");
        }
        basicControls.Camera.LClick.started += LClickDown;
        basicControls.Camera.LClick.canceled += LClickUp;
    }

    private void Holding(InputAction.CallbackContext context) {
        
    }

    private void LClickUp(InputAction.CallbackContext context)
    {
        dragDown=false;
    }

    private void LClickDown(InputAction.CallbackContext context)
    {
        dragDown=true;
    }



    // Update is called once per frame
    void Update()
    {
        if(PlayerManager.inst.uILock || EventSystem.current.IsPointerOverGameObject()) {
            return;
        }
        float xPos = gameCamera.position.x,zPos = gameCamera.position.z;
        if(dragDown) {
            float cameraRot = (gameCamera.rotation.eulerAngles.y-180)*Mathf.Deg2Rad;
            Vector2 delta = -1 * moveSenitivity * Time.deltaTime * basicControls.Camera.MouseMove.ReadValue<Vector2>();
            // print(delta);
            // print(cameraRot*Mathf.Rad2Deg);
            delta = new Vector2(
                -1* delta.x * Mathf.Cos(cameraRot) - delta.y * Mathf.Sin(cameraRot),
                delta.x * Mathf.Sin(cameraRot) - delta.y * Mathf.Cos(cameraRot)
            );
            // print(delta);
            // print("====");
            xPos+= delta.x;
            zPos+= delta.y;

            xPos = Mathf.Clamp(xPos, cameraTLCorner.x,cameraBRCorner.x);
            zPos = Mathf.Clamp(zPos, cameraTLCorner.y,cameraBRCorner.y);
        }

        float yVal = basicControls.Camera.Scroll.ReadValue<Vector2>().y;
        float yPos = gameCamera.position.y;
        yPos+=yVal * Time.deltaTime * scrollSensitivity * -1;
        yPos = Mathf.Clamp(yPos, cameraHRange.x,cameraHRange.y);
        gameCamera.position = new Vector3(xPos,yPos,zPos);
    }
}
