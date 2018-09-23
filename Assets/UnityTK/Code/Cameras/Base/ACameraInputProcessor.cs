using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    public abstract class ACameraModeInputProcessor : UTKCameraComponent, ICameraModeInputProcessor
    {
        public abstract Vector2 EvaluteLookAxis();
        public abstract Vector3 EvaluteMovementAxis();
        public abstract void UpdateInput(List<Vector3> movementAxis, List<Vector2> lookAxis);
    }
}
