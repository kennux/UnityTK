using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Camera mode input processor.
    /// Post processes all outputs from <see cref="ICameraInput"/>
    /// </summary>
    public interface ICameraModeInputProcessor
    {
        /// <summary>
        /// Updates the input in this input processor.
        /// The input updated here can be evaluate by:
        /// <see cref="EvaluteMovementAxis"/>
        /// <see cref="EvaluteLookAxis"/>
        /// 
        /// The lists may be reused so the input should be copied.
        /// </summary>
        /// <param name="movementAxis">All inputs from <see cref="ICameraInput.GetMovementAxis"/></param>
        /// <param name="lookAxis">All inputs from <see cref="ICameraInput.GetLookAxis"/></param>
        void UpdateInput(List<Vector3> movementAxis, List<Vector2> lookAxis);

        /// <summary>
        /// Evaluates the movement axis based on the inputs supplied in <see cref="UpdateInput(List{Vector3}, List{Vector3})"/>
        /// </summary>
        /// <returns>The evaluated movement axis in input space: <see cref="ICameraInput.GetMovementAxis"/></returns>
        Vector3 EvaluteMovementAxis();

        /// <summary>
        /// Evaluates the look axis based on the inputs supplied in <see cref="UpdateInput(List{Vector3}, List{Vector3})"/>
        /// </summary>
        /// <returns>The evaluated look axis in input space: <see cref="ICameraInput.GetLookAxis"/></returns>
        Vector2 EvaluteLookAxis();
    }
}
