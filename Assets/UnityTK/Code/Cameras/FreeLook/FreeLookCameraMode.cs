using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Cameras
{
    public class FreeLookCameraMode : CameraModeBase<FreeLookCameraModeInputData>
    {
        public float sensitivity = 10;

        protected override FreeLookCameraModeInputData MergeInputData(Dictionary<CameraModeInput<FreeLookCameraModeInputData>, FreeLookCameraModeInputData> data)
        {
            FreeLookCameraModeInputData fli = new FreeLookCameraModeInputData();
            foreach (var val in data.Values)
            {
                fli.lookAxis += val.lookAxis;
            }

            return fli;
        }

        protected override void _UpdateMode(Camera camera)
        {
            var rot = camera.transform.localRotation;

            float newRotationX = rot.eulerAngles.x - this.inputData.lookAxis.y * sensitivity;
            float newRotationY = rot.eulerAngles.y + this.inputData.lookAxis.x * sensitivity;

            camera.transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, rot.eulerAngles.z);
        }
    }
}
