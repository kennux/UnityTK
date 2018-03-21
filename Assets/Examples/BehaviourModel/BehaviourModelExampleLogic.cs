using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.BehaviourModel;

public class BehaviourModelExampleLogic : MonoBehaviour
{
    public BehaviourModelExample model;

    [Header("Debug")]
    [SerializeField]
    private bool isRotating = false;
    [SerializeField]
    private bool isJumping = false;
    [SerializeField]
    private float jump = 0;
    [SerializeField]
    private float jumpDir = 1;

    public void Awake()
    {
        this.model.euler.SetGetter(() => this.transform.eulerAngles);
        this.model.rotateOnce.handler += () => { this.transform.Rotate(Vector3.one, 100f); };

        this.model.rotate.RegisterActivityGetter(() => isRotating);
        this.model.rotate.RegisterStartCondition(() => !isRotating && !isJumping);
        this.model.rotate.RegisterStopCondition(() => isRotating);
        this.model.rotate.onStart += () => this.isRotating = true;
        this.model.rotate.onStop += () => this.isRotating = false;

        this.model.jump.RegisterCondition(() => !this.isRotating && !this.isJumping);
        this.model.jump.onFire += () =>
        {
            this.isJumping = true;
            this.jump = 0;
            this.jumpDir = 1;
        };
    }

    public void Update()
    {
        if (this.isRotating)
            this.transform.Rotate(Vector3.one, this.model.rotationSpeed.Get() * Time.deltaTime);

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
