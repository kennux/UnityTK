using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    public class AccumulationInputProcessor : ACameraModeInputProcessor
    {
        private Vector3 _movementAxis;
        private Vector2 _lookAxis;

        public override Vector2 EvaluteLookAxis()
        {
            return this._lookAxis;
        }

        public override Vector3 EvaluteMovementAxis()
        {
            return this._movementAxis;
        }

        public override void UpdateInput(List<Vector3> movementAxis, List<Vector2> lookAxis)
        {
            this._movementAxis = Vector3.zero;
            this._lookAxis = Vector2.zero;

            for (int i = 0; i < movementAxis.Count; i++)
            {
                this._movementAxis += movementAxis[i];
                this._lookAxis = lookAxis[i];
            }
        }
    }
}
