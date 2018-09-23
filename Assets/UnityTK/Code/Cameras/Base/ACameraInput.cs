using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    public abstract class ACameraInput : UTKCameraComponent, ICameraInput
    {
        public abstract Vector2 GetLookAxis();
        public abstract Vector3 GetMovementAxis();
    }
}
