using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using System.Threading;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using System;
using System.Diagnostics;
public class D3Cs : MonoBehaviour
{
  // public Texture3D texture;
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
Vector3[] posar;
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
public AsyncGPUReadbackRequest readbackRequest;
 public AsyncGPUReadbackRequest readbackRequestc;
     Color[] colors;
     Vector3 camrel;

    struct voxel{
     public   Vector3 pos;
     public   Color color;
    }
 void RunMain(AsyncGPUReadbackRequest r){
print("suc");


tmp.GetComponent<TextMeshProUGUI>().text = (1f / Time.unscaledDeltaTime).ToString();



    cg.SetInt("Size",size) ;    
    camrel = Camera.main.transform.position + new Vector3(50,50,50);
    cg.SetFloats("Cameraposition",camrel.x,camrel.y,camrel.z) ;        
    cg.SetBuffer(0,"Result",texpix);
    cg.SetBuffer(0,"ResultFinal",final);
    cg.SetBuffer(0,"Meta",meta);
    cg.Dispatch(0,size /8,size /8,size /8);
    
               
  validpos.Clear();
  validcolor.Clear();

 
      var curp = r.GetData<voxel>();
      print(curp.Length);


  readbackRequest = AsyncGPUReadback.Request(final,RunMain);

    Stopwatch stopWatch = new Stopwatch();
   stopWatch.Start();
    
     for (int i = 0; i < curp.Length; i++) //100ms !
      {
   if(curp[i].pos.z != 1000){
  validpos.Add(curp[i].pos);
    validcolor.Add(curp[i].color);
    }
    
    else{
        i += (int)curp[i].pos.x;
    }

        }


        stopWatch.Stop();
        print(stopWatch.ElapsedMilliseconds);
        
      

        positionBuffer.Dispose();

  positionBuffer = new ComputeBuffer(validpos.Count, sizeof(float) * 3);
  
     positionBuffer.SetData(validpos);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);

         colbuff.Dispose();
   colbuff = new ComputeBuffer(validpos.Count, sizeof(float) * 4);
 colbuff.SetData(validcolor);
        instanceMaterial.SetBuffer("colors", colbuff);

 }
    void Start()
    {
     //   print( sizeof(float)*7);
           colors = new Color[size * size * size];
  posar = new Vector3[size*size*size];
  int bsize = sizeof(float) * 4 ;
    int csize = sizeof(float) * 4 ;
     //  int dsize = sizeof(float) * 3 ;
texpix = new ComputeBuffer(size *size *size,bsize);
meta = new ComputeBuffer(size *size *size,csize);
final = new ComputeBuffer(size *size *size,sizeof(float)*7 );
//finalp = new ComputeBuffer(size *size *size,dsize);

texpix.SetData(colors);
//Renderer
     argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();

  //  print(Time.time);


    //compute
          cg.SetInt("Size",size) ;    
          camrel = Camera.main.transform.position + new Vector3(50,50,50);
          cg.SetFloats("Cameraposition",camrel.x,camrel.y,camrel.z) ;        
          cg.SetBuffer(0,"Result",texpix);
          cg.SetBuffer(0,"ResultFinal",final);
          cg.SetBuffer(0,"Meta",meta);
          cg.Dispatch(0,size /8,size /8,size /8);
          
 
            // extract
          readbackRequest = AsyncGPUReadback.Request(final,RunMain);

  
    }

 
    // Update is called once per frame
    void Update()
    {
        if(validpos.Count != 0){
tmp.GetComponent<TextMeshProUGUI>().text = (1f / Time.unscaledDeltaTime).ToString();


        positionBuffer.Dispose();

  positionBuffer = new ComputeBuffer(validpos.Count, sizeof(float) * 3);
  
     positionBuffer.SetData(validpos);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);

         colbuff.Dispose();
   colbuff = new ComputeBuffer(validpos.Count, sizeof(float) * 4);
 colbuff.SetData(validcolor);
        instanceMaterial.SetBuffer("colors", colbuff);



 

    if(instanceCount -1000 < validcolor.Count){
            instanceCount = validcolor.Count + 5000;
        }

        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
            UpdateBuffers();

  
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
       instanceMaterial.SetBuffer("colors", colbuff);

  
        }


}



   
  
  




 void UpdateBuffers() {
//   print("bufup");
 
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
