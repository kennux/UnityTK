using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Camera mode feature implementation (<see cref="ICameraModeShakeFeature"/>).
    /// <see cref="FreeLookCameraMode"/>
    /// </summary>
    public class FreeLookCameraModeShakeFeature : CameraModeFeatureBase, ICameraModeShakeFeature
    {
        /// <summary>
        /// The spring damping.
        /// </summary>
        public Vector3 damping;

        /// <summary>
        /// The spring stiffness.
        /// </summary>
        public Vector3 stiffness;

        /// <summary>
        /// The spring instance.
        /// </summary>
        public SpringPhysics spring;

        public void Awake()
        {
            this.spring = new SpringPhysics(this.stiffness, this.damping);
        }

        public override void PostProcessState(ref CameraState cameraState)
        {
            cameraState.transform.localPosition += cameraState.transform.localRotation * this.spring.Get(); 
        }

        public void FixedUpdate()
        {
            this.spring.FixedUpdate();
        }

        public void Shake(Vector2 force)
        {
            this.spring.AddForce(force);
        }
    }
}
