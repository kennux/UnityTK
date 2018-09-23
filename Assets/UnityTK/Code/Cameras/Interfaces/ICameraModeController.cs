using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    public interface ICameraModeController
    {
        /// <summary>
        /// Called from camera mode in order to update the controlled with processed input from multiple <see cref="ICameraModeInputProcessor"/>.
        /// Processed inputs are accumulated.
        /// </summary>
        /// <param name="movementAxis"></param>
        /// <param name="lookAxis"></param>
        void UpdateController(Vector3 movementAxis, Vector2 lookAxis, ref CameraModeData cameraModeData);
    }
}
