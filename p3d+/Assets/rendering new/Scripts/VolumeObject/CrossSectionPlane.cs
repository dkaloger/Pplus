﻿using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Cross section plane.
    /// Used for cutting a model (cross section view).
    /// </summary>
    [ExecuteInEditMode]
    public class CrossSectionPlane : MonoBehaviour
    {
        /// <summary>
        /// Volume dataset to cross section.
        /// </summary>
        public VolumeRenderedObject targetObject;

        private void OnDisable()
        {
            if (targetObject != null)
                targetObject.meshRenderer.sharedMaterial.DisableKeyword("SLICEPLANE_ON");
        }

        private void Update()
        {
            if (targetObject == null)
                return;

            Material mat = targetObject.meshRenderer.sharedMaterial;

            mat.EnableKeyword("SLICEPLANE_ON");
            mat.SetVector("_PlanePos", targetObject.transform.position - transform.position);
            mat.SetVector("_PlaneNormal", transform.forward);
        }
    }
}
