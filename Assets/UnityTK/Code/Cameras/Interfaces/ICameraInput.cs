using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Interface defining behaviour for implementing input from the user to camera.
    /// </summary>
    public interface ICameraInput
    {
        /// <summary>
        /// The input movment axis.
        /// X = Left/Right
        /// Y = Up/Down
        /// Z = Forward/Back
        /// 
        /// Axis is in input coordinate space, not screenspace
        /// </summary>
        /// <returns>The movment axis for the camera.</returns>
        Vector3 GetMovementAxis();

        /// <summary>
        /// The input look axis.
        /// X = Left/Right
        /// Y = Up/Down
        /// 
        /// Axis is in input coordinate space, not screenspace
        /// </summary>
        /// <returns>The look axis for the camera.</returns>
        Vector2 GetLookAxis();
    }
}
