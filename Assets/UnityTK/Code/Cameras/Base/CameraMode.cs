using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Abstract base class for implementing a camera mode.
    /// Camera modes contain the logic for camera behaviour of <see cref="UTKCamera"/>.
    /// 
    /// This is only a base class, for implementing new camera modes <see cref="CameraModeBase{TInputData}"/>
    /// </summary>
    public abstract class CameraMode : UTKCameraComponent
    {
        /// <summary>
        /// Called in order to updaate the camera mode.
        /// Should be called every frame.
        /// </summary>
        /// <param name="camera">The camera to run the update on.</param>
        public abstract void UpdateMode(Camera camera);
    }
}
