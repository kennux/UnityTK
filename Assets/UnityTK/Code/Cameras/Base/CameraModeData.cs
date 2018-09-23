using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// A base implementation for any camera mode data objects to be implemented.
    /// </summary>
    public struct CameraModeData
    {
        /// <summary>
        /// The position of the camera in local space.
        /// </summary>
        public Vector3 localPosition;

        /// <summary>
        /// The rotation of the camera in local space.
        /// </summary>
        public Quaternion localRotation;

        /// <summary>
        /// The camera projection matrix.
        /// </summary>
        public Matrix4x4? projectionMatrix;

        /// <summary>
        /// Writes camera data in this struct to the specified camera.
        /// </summary>
        /// <param name="camera">The camera to write data to.</param>
        public void WriteTo(Camera camera)
        {
            camera.transform.localPosition = this.localPosition;
            camera.transform.localRotation = this.localRotation;

            if (!this.projectionMatrix.HasValue)
                camera.ResetProjectionMatrix();
            else
                camera.projectionMatrix = this.projectionMatrix.Value;
        }

        /// <summary>
        /// Reads data for this struct from the specified camera.
        /// </summary>
        /// <param name="camera">The camera </param>
        public void ReadFrom(Camera camera)
        {
            this.localPosition = camera.transform.localPosition;
            this.localRotation = camera.transform.localRotation;

            // TODO: this.projectionMatrix = camera.projectionMatrix;
        }
    }
}
