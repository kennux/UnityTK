using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Cameras
{
    public abstract class CameraMode : UTKCameraComponent
    {
        protected Vector3 movementAxis;
        protected Vector2 lookAxis;

        public abstract void UpdateMode(Camera camera);

        public virtual void UpdateInput(Dictionary<CameraInput, Vector3> movementAxis, Dictionary<CameraInput, Vector2> lookAxis)
        {
            Vector3 mA = Vector3.zero;
            Vector2 lA = Vector2.zero;

            foreach (var v in movementAxis.Values)
                mA += v;

            foreach (var v in lookAxis.Values)
                lA += v;

            this.movementAxis = mA;
            this.lookAxis = lA;
        }
    }
}
