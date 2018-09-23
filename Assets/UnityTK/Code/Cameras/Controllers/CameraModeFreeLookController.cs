using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Cameras
{
    public class CameraModeFreeLookController : ACameraModeController
    {
        public float sensitivity = 10;

        public override void UpdateController(Vector3 movementAxis, Vector2 lookAxis, ref CameraModeData cameraModeData)
        {
            var rot = cameraModeData.localRotation;

            float newRotationX = rot.eulerAngles.x - lookAxis.y * sensitivity;
            float newRotationY = rot.eulerAngles.y + lookAxis.x * sensitivity;

            cameraModeData.localRotation = Quaternion.Euler(newRotationX, newRotationY, rot.eulerAngles.z);
        }
    }
}
