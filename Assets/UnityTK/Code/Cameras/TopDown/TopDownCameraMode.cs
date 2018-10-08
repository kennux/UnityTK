using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Camera mode implementation for top down cameras.
    /// This includes traditional top down cameras aswell as isometric camera perspectives (TODO).
    /// 
    /// It implements camera movement along a 2d plane in 3d space, which can be moved on the y-axis withing certain limits to create the zoom behaviour.
    /// </summary>
    public class TopDownCameraMode : CameraModeBase<TopDownCameraModeInputData>
    {
        /// <summary>
        /// The bounding box of the world to be viewed with this camera.
        /// This controls the camera plane used for movement.
        /// After changing this at runtime, call <see cref="UpdatePlane"/> explicitly.
        /// </summary>
        [Header("Config")]
        public Bounds bounds;

        /// <summary>
        /// Y-axis min of <see cref="bounds"/>
        /// </summary>
        public float minYLevel
        {
            get { return this.bounds.min.y; }
        }

        /// <summary>
        /// Y-axis max of <see cref="bounds"/>
        /// </summary>
        public float maxYLevel
        {
            get { return this.bounds.max.y; }
        }

        /// <summary>
        /// Euler angles to be always set to the camera.
        /// </summary>
        public Vector3 eulerAngles = new Vector3(90, 0, 0);

        /// <summary>
        /// The movement sensitivity (multiplicator)
        /// </summary>
        public float movementSensitivity = 100f;

        /// <summary>
        /// The zoom sensitivity (multiplicator)
        /// </summary>
        public float zoomSensitivity= 15f;

        /// <summary>
        /// The zoom level in worldspace (y-axis).
        /// </summary>
        protected float zoomLevel
        {
            get { return this.minYLevel + (this.maxYLevel * this.zoomLevelNormalized); }
        }

        /// <summary>
        /// The normalized zoomlevel, normalized between <see cref="minYLevel"/>, <see cref="maxYLevel"/>
        /// </summary>
        protected float zoomLevelNormalized;

        /// <summary>
        /// The min vector of the 2d plane used for movement.
        /// </summary>
        protected Vector2 planeMin;

        /// <summary>
        /// The mmax vector of the 2d plane used for movement.
        /// </summary>
        protected Vector2 planeMax;

        /// <summary>
        /// The camera position on a plane defined by <see cref="planeMin"/> and <see cref="planeMax"/>
        /// </summary>
        protected Vector2 planeCoords;

        protected override TopDownCameraModeInputData MergeInputData(Dictionary<CameraModeInput<TopDownCameraModeInputData>, TopDownCameraModeInputData> data)
        {
            TopDownCameraModeInputData id = new TopDownCameraModeInputData();
            foreach (var d in data.Values)
            {
                id.movementDelta += d.movementDelta;
                id.zoomDelta += d.zoomDelta;
            }

            return id;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(this.bounds.center, this.bounds.size);
        }

        /// <summary>
        /// Updates the internal movement plane.
        /// <seealso cref="bounds"/>
        /// </summary>
        public void UpdatePlane()
        {
            this.planeMin = new Vector2(bounds.min.x, bounds.min.z);
            this.planeMax = new Vector2(bounds.max.x, bounds.max.z);
        }

        private void OnEnable()
        {
            UpdatePlane();
        }

        public override void OnPrepare(CameraState camera)
        {
            base.OnPrepare(camera);

            // Try to get plane coords and clamp them
            this.planeCoords = new Vector2(camera.transform.position.x, camera.transform.position.z);
            this.zoomLevelNormalized = camera.transform.position.y.Remap(this.minYLevel, this.maxYLevel, 0, 1);
            ClampState();
        }

        /// <summary>
        /// Clamps:
        /// - <see cref="planeCoords"/> to <see cref="planeMin"/> and <see cref="planeMax"/>
        /// - <see cref="zoomLevelNormalized"/> to 0-1
        /// </summary>
        private void ClampState()
        {
            this.planeCoords.x = Mathf.Clamp(this.planeCoords.x, planeMin.x, planeMax.x);
            this.planeCoords.y = Mathf.Clamp(this.planeCoords.y, planeMin.y, planeMax.y);
            this.zoomLevelNormalized = Mathf.Clamp01(this.zoomLevelNormalized);
        }

        protected override void _UpdateMode(ref CameraState cameraState)
        {
            // Logical update
            this.planeCoords += this.inputData.movementDelta * movementSensitivity * Time.deltaTime;
            this.zoomLevelNormalized += this.inputData.zoomDelta * zoomSensitivity * Time.deltaTime;
            ClampState();

            // Visual update
            cameraState.transform.position = new Vector3(this.planeCoords.x, this.zoomLevel, this.planeCoords.y);
            cameraState.transform.rotation = Quaternion.Euler(this.eulerAngles);
        }
    }
}