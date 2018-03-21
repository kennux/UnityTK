using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.BehaviourModel;

public class BehaviourModelExampleController : MonoBehaviour
{
    public BehaviourModelExample model;

    private bool speedOverridden = false;

    public void Start()
    {
        this.model.rotationSpeed.AddOverrideEvaluator(this.SpeedOverrideEvaluator, 0);
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
            if (!this.model.rotate.IsActive())
                this.model.rotate.TryStart();
        }
        else if (this.model.rotate.IsActive())
        {
            this.model.rotate.TryStop();
        }

        if (Input.GetKeyDown(KeyCode.Space))
            this.model.jump.Try();

        if (Input.GetKeyDown(KeyCode.I))
            this.speedOverridden = !this.speedOverridden;

        if (Input.GetKeyDown(KeyCode.O))
            this.model.rotateOnce.Fire();

        if (Input.GetKeyDown(KeyCode.P))
            Debug.Log(this.model.euler.Get());
    }

    private float SpeedOverrideEvaluator(float val)
    {
        if (this.speedOverridden)
            return val * 5f;
        return val;
    }
}
