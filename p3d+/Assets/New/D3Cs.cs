using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
public class D3Cs : MonoBehaviour
{
   public Texture3D texture;
 public  int size ;
   public ComputeShader cg;

   public ComputeBuffer texpix;
    public ComputeBuffer meta;
    public ComputeBuffer final;
      public ComputeBuffer finalp;
          public ComputeBuffer colbuff;
  public GameObject game;

  public Material material;

  public GameObject game2;

public GameObject tmp;
Vector3 [] posar;
 List<Vector3> validpos = new List<Vector3>();
  List<Color> validcolor = new List<Color>();
 //renderer 
   public int instanceCount = 100000;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;

    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

 
  
 
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
  posar = new Vector3[1000000];
  texture = make3dtex();
  int bsize = sizeof(float) * 4 ;
    int csize = sizeof(float) * 4 ;
       int dsize = sizeof(float) * 3 ;
texpix = new ComputeBuffer(size *size *size,bsize);
meta = new ComputeBuffer(size *size *size,csize);
final = new ComputeBuffer(size *size *size,bsize);
finalp = new ComputeBuffer(size *size *size,dsize);

texpix.SetData(colors);
//Renderer
     argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }


    // Update is called once per frame
    void Update()
    {
tmp.GetComponent<TextMeshProUGUI>().text = (1f / Time.unscaledDeltaTime).ToString();

    cg.SetInt("Size",size) ;    

  Vector3 camrel = Camera.main.transform.position + new Vector3(50,50,50);
  print(camrel);

    cg.SetFloats("Cameraposition",camrel.x,camrel.y,camrel.z) ;        
   cg.SetBuffer(0,"Result",texpix);
   cg.SetBuffer(0,"ResultFinal",final);
      cg.SetBuffer(0,"PositionsFinal",finalp);
    cg.SetBuffer(0,"Meta",meta);
  cg.Dispatch(0,size /10,size /10,size /10);

     final.GetData(colors);

     finalp.GetData(posar);
  //may cause memory leak   texpix.Dispose();
   texture.SetPixels(colors);
   texture.Apply();  
     
             texture.SetPixels(colors);
        // gamep.texture = texture;
   game.GetComponent<MeshRenderer>().material.SetTexture("_GradientTex",null) ;





   //RENDERER 

  // Update starting position buffer
        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
            UpdateBuffers();

        // Pad input
   //   if (Input.GetAxisRaw("Horizontal") != 0.0f)
        //    instanceCount = (int)Mathf.Clamp(instanceCount + Input.GetAxis("Horizontal") * 40000, 1.0f, 5000000.0f);

        // Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
   
  
 
    // Use this for initialization

 
    // Update is called once per frame

  validpos.Clear();
  validcolor.Clear();
     for (int i = 0; i < size*size*size; i++) {
      
      //     Graphics.DrawMesh (wrapper.mesh, wrapper.location, Quaternion.identity, cubeMaterial, 0);
   if(posar[i].z != 10000){
// Graphics.DrawMesh (Cube, posar[i], Quaternion.identity, cubeMaterial, 0);
    validpos.Add(posar[i]);
    validcolor.Add(colors[i]);
    }
        }if(instanceCount -1000 < validcolor.Count){
            instanceCount= validcolor.Count + 5000;
        }
      //  instanceCount = 30000;
    //  print();
       positionBuffer.SetData(validpos);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);
 colbuff.SetData(validcolor);
        instanceMaterial.SetBuffer("colors", colbuff);

    
  }

      void OnGUI() {
        GUI.Label(new Rect(265, 25, 200, 30), "Instance Count: " + instanceCount.ToString());
        instanceCount = (int)GUI.HorizontalSlider(new Rect(25, 20, 200, 30), (float)instanceCount, 1.0f, 5000000.0f);
    }



 void UpdateBuffers() {
        // Ensure submesh index is in range
        if (instanceMesh != null)
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

        // Positions
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 3);
        colbuff = new ComputeBuffer(instanceCount, sizeof(float) * 4);
     
       Vector3[] positions = new Vector3[instanceCount];

     

        positionBuffer.SetData(validpos);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);

        colbuff.SetData(validcolor);
        instanceMaterial.SetBuffer("colors", colbuff);

        // Indirect args
        if (instanceMesh != null) {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }

    void OnDisable() {
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }



}
