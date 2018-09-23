using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Cameras
{
    [RequireComponent(typeof(CameraMode))]
    public abstract class ACameraModeController : UTKCameraComponent, ICameraModeController
    {
        public abstract void UpdateController(Vector3 movementAxis, Vector2 lookAxis, ref CameraModeData cameraModeData);
    }
}
