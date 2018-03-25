using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.BehaviourModel;

public class BehaviourModelExampleController : BehaviourModelMechanicComponent<BehaviourModelExampleMechanic>
{
    private bool speedOverridden = false;

    public void Start()
    {
        this.mechanic.rotationSpeed.AddOverrideEvaluator(this.SpeedOverrideEvaluator, 0);
    }

    public void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.R))
            this.model.rotate.TryStart();
        if (Input.GetKeyUp(KeyCode.R))
            this.model.rotate.TryStop();
        */
        if (Input.GetKey(KeyCode.R))
        {
            if (!this.mechanic.rotate.IsActive())
                this.mechanic.rotate.TryStart();
        }
        else if (this.mechanic.rotate.IsActive())
        {
            this.mechanic.rotate.TryStop();
        }

        if (Input.GetKeyDown(KeyCode.Space))
            this.mechanic.jump.Try();

        if (Input.GetKeyDown(KeyCode.I))
            this.speedOverridden = !this.speedOverridden;

        if (Input.GetKeyDown(KeyCode.O))
            this.mechanic.rotateOnce.Fire();

        if (Input.GetKeyDown(KeyCode.P))
            Debug.Log(this.mechanic.euler.Get());
    }

    private float SpeedOverrideEvaluator(float val)
    {
        if (this.speedOverridden)
            return val * 5f;
        return val;
    }
}
