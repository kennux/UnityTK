using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    public class CameraMode : UTKCameraComponent
    {
        public CameraModeData cameraModeData
        {
            get { return this._cameraModeData; }
        }

        protected CameraModeData _cameraModeData;

        /// <summary>
        /// All input processors of this camera mode.
        /// </summary>
        protected List<ICameraModeInputProcessor> inputProcessors = new List<ICameraModeInputProcessor>();

        /// <summary>
        /// All controllers of this camera mode.
        /// </summary>
        protected List<ICameraModeController> controllers = new List<ICameraModeController>();

        private void Awake()
        {
            this.GetComponentsInChildren<ICameraModeInputProcessor>(inputProcessors);
            this.GetComponentsInChildren<ICameraModeController>(controllers);
        }

        public CameraModeData UpdateMode(List<Vector3> movementAxis, List<Vector2> lookAxis)
        {
            Vector3 _movementAxis = Vector3.zero;
            Vector2 _lookAxis = Vector2.zero;

            // Collect processed input
            foreach (var processor in this.inputProcessors)
            {
                processor.UpdateInput(movementAxis, lookAxis);
                _movementAxis += processor.EvaluteMovementAxis();
                _lookAxis += processor.EvaluteLookAxis();
            }

            // Update controllers
            foreach (var controller in this.controllers)
            {
                controller.UpdateController(_movementAxis, _lookAxis, ref this._cameraModeData);
            }

            return this._cameraModeData;
        }

        public void Setup()
        {
            this._cameraModeData = new CameraModeData();
        }
    }
}
