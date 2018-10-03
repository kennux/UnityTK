using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Cameras
{
    public class FreeLookCameraMode : CameraMode 
    {
        public float sensitivity = 10;

        public override void UpdateMode(Camera camera)
        {
            var rot = camera.transform.localRotation;

            float newRotationX = rot.eulerAngles.x - lookAxis.y * sensitivity;
            float newRotationY = rot.eulerAngles.y + lookAxis.x * sensitivity;

            camera.transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, rot.eulerAngles.z);
        }
    }
}
