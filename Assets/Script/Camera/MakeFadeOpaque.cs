using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeFadeOpaque : MonoBehaviour
{
    public void SetFaded(List<Material> materials, Material _nonURP)
    {
        List<Material> newMats = new List<Material>();
        foreach (var material in materials)
        {
            if (false)
            {
                Material newMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                newMat.CopyPropertiesFromMaterial(material);
                newMat.SetFloat("_Surface", 1);
                newMat.SetFloat("_Blend", (float)UnityEngine.Rendering.BlendMode.One); // Additive blending
                newMat.DisableKeyword("_ALPHATEST_ON");
                newMat.EnableKeyword("_ALPHABLEND_ON");
                newMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                newMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                newMat.EnableKeyword("_NORMALMAP");
                newMat.EnableKeyword("_DETAIL_MULX2");
                newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                newMat.SetInt("_SrcBlendAlpha", (int)UnityEngine.Rendering.BlendMode.One);
                newMat.SetInt("_DstBlendAlpha", (int)UnityEngine.Rendering.BlendMode.One);
                newMat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                newMat.SetInt("_BlendOpAlpha", (int)UnityEngine.Rendering.BlendOp.Add);
                newMats.Add(newMat);
            }
            else //if (material.shader.name.Contains("Shader Graphs/"))
            {
                Material newMat = new Material(_nonURP);
                newMats.Add(newMat);
            }
        }

        if (gameObject.TryGetComponent(out Renderer r))
            r.materials = newMats.ToArray();

        
    }

    public void SetOpaque(List<Material> materials)
    {

    }
}
