using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.BehaviourModel;

public class BehaviourModelExampleLogic : BehaviourModelMechanicComponent<BehaviourModelExampleMechanic>
{
    [Header("Debug")]
    [SerializeField]
    private bool isRotating = false;
    [SerializeField]
    private bool isJumping = false;
    [SerializeField]
    private float jump = 0;
    [SerializeField]
    private float jumpDir = 1;

    protected override void Awake()
    {
        base.Awake();
        this.mechanic.euler.SetGetter(() => this.transform.eulerAngles);
        this.mechanic.rotateOnce.handler += () => { this.transform.Rotate(Vector3.one, 100f); };

        this.mechanic.rotate.RegisterActivityGetter(() => isRotating);
        this.mechanic.rotate.RegisterStartCondition(() => !isJumping);
        this.mechanic.rotate.RegisterStopCondition(() => isRotating);
        this.mechanic.rotate.onStart += () => this.isRotating = true;
        this.mechanic.rotate.onStop += () => this.isRotating = false;

        this.mechanic.jump.RegisterCondition(() => !this.isRotating && !this.isJumping);
        this.mechanic.jump.onFire += () =>
        {
            this.isJumping = true;
            this.jump = 0;
            this.jumpDir = 1;
        };
    }

    public void Update()
    {
        if (this.isRotating)
            this.transform.Rotate(Vector3.one, this.mechanic.rotationSpeed.Get() * Time.deltaTime);

        if (this.isJumping)
        {
            this.transform.position = new Vector3(this.transform.position.x, this.jump, this.transform.position.z);
            this.jump += (this.jumpDir * Time.deltaTime);
            if (this.jump > 1)
            {
                this.jumpDir = -1;
            }

            if (this.jumpDir == -1 && this.jump <= 0)
            {
                this.jump = 0;
                this.isJumping = false;
                this.transform.position = new Vector3(this.transform.position.x, this.jump, this.transform.position.z);
            }
        }
    }
}
