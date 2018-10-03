using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// UnityTK camera class.
    /// This is the base component for any cameras using the UnityTK camera module.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class UTKCamera : MonoBehaviour
    {
        /// <summary>
        /// The current camera mode that is used / will be used on awake.
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
        public ReadOnlyList<CameraInput> inputs
        {
            get { return this._inputs; }
        }
        private List<CameraInput> _inputs = new List<CameraInput>();

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
            this.GetComponentsInChildren<CameraInput>(this._inputs);

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
                this.currentMode = this.modes[0];

            SetCameraMode(this.currentMode);
        }

        /// <summary>
        /// Updates the camera :>
        /// </summary>
        private void Update()
        {
            Dictionary<CameraInput, Vector3> movementAxis = DictionaryPool<CameraInput, Vector3>.Get();
            Dictionary<CameraInput, Vector2> lookAxis = DictionaryPool<CameraInput, Vector2>.Get();

            // Collect input
            foreach (var input in this._inputs)
            {
                movementAxis.Add(input, input.GetMovementAxis());
                lookAxis.Add(input, input.GetLookAxis());
            }

            // Update mode
            this.currentMode.UpdateInput(movementAxis, lookAxis);
            this.currentMode.UpdateMode(this.camera);

            DictionaryPool<CameraInput, Vector3>.Return(movementAxis);
            DictionaryPool<CameraInput, Vector2>.Return(lookAxis);
        }

        #endregion

        #region Camera modes

        /// <summary>
        /// Sets the specified camera mode by disabling the current mode and replacing it with the specified mode.
        /// Only nodes known by this camera can be used!
        /// </summary>
        /// <param name="mode">The camera mode to be set.</param>
        public void SetCameraMode(CameraMode mode)
        {
            if (!this._modes.Contains(mode))
                throw new System.ArgumentException(string.Format("Tried to set camera mode which isnt available to the camera: {0}", mode));

            if (!Essentials.UnityIsNull(this.currentMode))
                this.currentMode.gameObject.SetActive(false);

            this.currentMode = mode;
            this.currentMode.gameObject.SetActive(true);
        }

        #endregion
    }
}