using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Camera input implementation for a mouse.
    /// </summary>
    public class CameraMouseInput : CameraInput
    {
        public override Vector3 GetMovementAxis()
        {
            return Vector3.zero;
        }

        public override Vector2 GetLookAxis()
        {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
    }
}
