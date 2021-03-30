using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class D3Cs : MonoBehaviour
{
   public Texture3D texture;
 public  int size ;
   public ComputeShader cg;

   public ComputeBuffer texpix;
    public ComputeBuffer meta;
    public ComputeBuffer final;
  public GameObject game;
  public Reference gamep;
  public Material material;

  public GameObject game2;
 
    // Start is called before the first frame update
     Color[] colors;
    Texture3D make3dtex(){

         //  int size = 32;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode =  TextureWrapMode.Clamp;

        // Create the texture and apply the configuration
        Texture3D texture = new Texture3D(size, size, size, format, false);
        texture.wrapMode = wrapMode;

        // Create a 3-dimensional array to store color data
        colors = new Color[size * size * size];

        // Populate the array so that the x, y, and z values of the texture will map to red, blue, and green colors
        float inverseResolution = 1.0f / (size - 1.0f);
     
        // Copy the color values to the texture
        texture.SetPixels(colors);

        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();        

        return texture;
        
    }
    void Start()
    {
  
  texture = make3dtex();
  int bsize = sizeof(float) * 4 ;
    int csize = sizeof(float) * 4 ;
texpix = new ComputeBuffer(size *size *size,bsize);
meta = new ComputeBuffer(size *size *size,csize);
final = new ComputeBuffer(size *size *size,bsize);
texpix.SetData(colors);

    }


    // Update is called once per frame
    void Update()
    {
    cg.SetInt("Size",size) ;    

  Vector3 camrel = Camera.main.transform.position + new Vector3(50,50,50);
  print(camrel);
  
    cg.SetFloats("Cameraposition",camrel.x,camrel.y,camrel.z) ;        
   cg.SetBuffer(0,"Result",texpix);
   cg.SetBuffer(0,"ResultFinal",final);
    cg.SetBuffer(0,"Meta",meta);
  cg.Dispatch(0,size /10,size /10,size /10);

     final.GetData(colors);

  //may cause memory leak   texpix.Dispose();
   texture.SetPixels(colors);
   texture.Apply();  
     
             texture.SetPixels(colors);
         gamep.texture = texture;
   game.GetComponent<MeshRenderer>().material.SetTexture("_GradientTex",null) ;
    }


}
