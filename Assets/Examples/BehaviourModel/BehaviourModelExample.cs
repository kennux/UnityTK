using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.BehaviourModel;

public class BehaviourModelExample : MonoBehaviour
{
    public ModifiableFloat rotationSpeed = new ModifiableFloat(10);
    public Activity rotate = new Activity();
    public AttemptEvent jump = new AttemptEvent();
    public MessageEvent rotateOnce = new MessageEvent();
    public ModelProperty<Vector3> euler = new ModelProperty<Vector3>();
}
