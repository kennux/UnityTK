using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.BehaviourModel;

public class BehaviourModelExampleMechanic : BehaviourModelMechanic
{
    public ModelModifiableFloat rotationSpeed = new ModelModifiableFloat(10);
    public ModelActivity rotate = new ModelActivity();
    public ModelAttempt jump = new ModelAttempt();
    public ModelEvent rotateOnce = new ModelEvent();
    public ModelProperty<Vector3> euler = new ModelProperty<Vector3>();

    protected override void SetupConstraints()
    {
        this.rotate.RegisterStartCondition(() => !this.rotate.IsActive());
    }
}
