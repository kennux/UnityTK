using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;

public class VisualRepresentationOverrides : MonoBehaviour
{
    public VisualRepresentation visualRepresentation;

    public Material materialToOverride;
    public Material overrideMaterial;
    private Material _materialToOverride;
    private Material _overrideMaterial;

    public Mesh meshToOverride;
    public Material overrideMeshMaterial;
    private Mesh _meshToOverride;
    private Material _overrideMeshMaterial;

    public void Update()
    {
        if (!ReferenceEquals(this._materialToOverride, this.materialToOverride) || !ReferenceEquals(this._overrideMaterial, this.overrideMaterial))
        {
            this.visualRepresentation.ClearMaterialOverrides();
            
            if (!Essentials.UnityIsNull(this.materialToOverride) && !Essentials.UnityIsNull(this.overrideMaterial))
                this.visualRepresentation.OverrideMaterial(this.materialToOverride, this.overrideMaterial);

            this._materialToOverride = this.materialToOverride;
            this._overrideMaterial = this.overrideMaterial;
        }

        if (!ReferenceEquals(this._meshToOverride, this.meshToOverride) || !ReferenceEquals(this._overrideMeshMaterial, this.overrideMeshMaterial))
        {
            this.visualRepresentation.ClearMeshMaterialOverrides();

            if (!Essentials.UnityIsNull(this.meshToOverride) && !Essentials.UnityIsNull(this.overrideMeshMaterial))
                this.visualRepresentation.OverrideMaterial(this.meshToOverride, this.overrideMeshMaterial);

            this._meshToOverride = this.meshToOverride;
            this._overrideMeshMaterial = this.overrideMeshMaterial;
        }
    }
}
