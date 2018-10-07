using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;
using UnityTK.Cameras;

public class CameraModeExampleSelector : MonoBehaviour
{
    public UTKCamera cam;
    public CameraMode[] modes;

    public float shakeMagnitude;

    public void SelectMode(int index)
    {
        this.cam.SetCameraMode(this.modes[index]);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Shake();
    }

    public void Shake()
    {
        var shakeFeature = this.cam.TryGetCameraModeFeature<ICameraModeShakeFeature>();
        if (!ReferenceEquals(shakeFeature, null))
            shakeFeature.Shake(Random.insideUnitCircle * this.shakeMagnitude);
    }
}
