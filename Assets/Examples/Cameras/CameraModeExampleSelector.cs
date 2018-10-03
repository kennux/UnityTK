using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;
using UnityTK.Cameras;

public class CameraModeExampleSelector : MonoBehaviour
{
    public UTKCamera cam;
    public CameraMode[] modes;

    public void SelectMode(int index)
    {
        this.cam.SetCameraMode(this.modes[index]);
    }
}
