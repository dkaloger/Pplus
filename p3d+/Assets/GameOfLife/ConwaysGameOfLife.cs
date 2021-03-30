using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConwaysGameOfLife : MonoBehaviour
{
 public  RenderTexture Tex1_3D;
   int noise1Gen;
 public  int tex1Res;
 public     ComputeShader noiseCompute;
  void Start()
{
        //Create 3D Render Texture 1
        Tex1_3D = new RenderTexture(tex1Res, tex1Res, 1);
        Tex1_3D.enableRandomWrite = true;
        Tex1_3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
      //  Tex1_3D.volumeDepth = tex1Res;
      Tex1_3D.depth = 0;
        Tex1_3D.Create();
        noise1Gen = noiseCompute.FindKernel("Noise1Gen");
        noiseCompute.SetTexture(noise1Gen, "Noise1", Tex1_3D);
}
 
public void Update()
{
        noiseCompute.Dispatch(noise1Gen, tex1Res / 8, tex1Res / 8, tex1Res / 8);
}

}
