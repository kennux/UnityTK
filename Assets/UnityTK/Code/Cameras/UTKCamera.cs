using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// UnityTK camera class.
    /// This is the base component for any cameras using the UnityTK camera module.
    /// </summary>
    public class UTKCamera : MonoBehaviour
    {
        /// <summary>
        /// The current camera mode to be used.
        /// </summary>
        [Header("Start Parameters")]
        public CameraMode currentMode;

        public new Camera camera
        {
            get { return this._camera.Get(this); }
        }
        private LazyLoadedComponentRef<Camera> _camera;

        /// <summary>
        /// All modes this camera has avilable.
        /// </summary>
        public ReadOnlyList<CameraMode> modes
        {
            get { return this._modes; }
        }
        private List<CameraMode> _modes = new List<CameraMode>();

        /// <summary>
        /// All input implementations this camera has available.
        /// </summary>
        public ReadOnlyList<ICameraInput> inputs
        {
            get { return this._inputs; }
        }
        private List<ICameraInput> _inputs = new List<ICameraInput>();

        #region Unity Messages

        /// <summary>
        /// Sets up the camera modes and inputs.
        /// <see cref="modes"/>
        /// <see cref="inputs"/>
        /// </summary>
        private void Awake()
        {
            // Read components
            this.GetComponentsInChildren<CameraMode>(this._modes);
            this.GetComponentsInChildren<ICameraInput>(this._inputs);

            // Validity
            if (this._modes.Count == 0)
            {
                Debug.LogError("UnityTK Camera without modes! Disabling camera!", this);
                this.enabled = false;
                return;
            }
            if (this._inputs.Count == 0)
            {
                Debug.LogError("UnityTK Camera without inputs! Disabling camera!", this);
                this.enabled = false;
                return;
            }

            // Set up mode if it isnt set at beginning
            if (Essentials.UnityIsNull(this.currentMode))
                this.currentMode = this._modes[0];
            this.InstantSetCameraMode(this.currentMode);
        }

        /// <summary>
        /// Updates the camera :>
        /// </summary>
        private void Update()
        {
            List<Vector3> movementAxis = ListPool<Vector3>.Get();
            List<Vector2> lookAxis = ListPool<Vector2>.Get();

            // Collect input
            foreach (var input in this._inputs)
            {
                movementAxis.Add(input.GetMovementAxis());
                lookAxis.Add(input.GetLookAxis());
            }

            // Update mode
            var data = this.currentMode.UpdateMode(movementAxis, lookAxis);

            // Apply data
            data.WriteTo(this.camera);

            ListPool<Vector3>.Return(movementAxis);
            ListPool<Vector2>.Return(lookAxis);
        }

        #endregion

        #region Camera modes

        /// <summary>
        /// Instantly sets the specified camera mode.
        /// </summary>
        /// <param name="mode">The camera mode to be set.</param>
        public void InstantSetCameraMode(CameraMode mode)
        {
            if (!this._modes.Contains(mode))
                throw new System.ArgumentException(string.Format("Tried to set camera mode which isnt available to the camera: {0}", mode));

            this.currentMode = mode;
            this.currentMode.Setup();
        }

        #endregion
    }
}