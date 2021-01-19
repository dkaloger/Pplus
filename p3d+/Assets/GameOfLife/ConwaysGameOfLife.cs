using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConwaysGameOfLife : MonoBehaviour
{
    public Texture input;

    public int width = 728;
    public int height = 485 * 2;

    public ComputeShader compute;
    public RenderTexture Input;
   
    public RenderTexture Output;

    public Material material;

    private int kernel;
    private bool pingPong;
    public int  T;


    public Texture2D texture;

    int count_x = 0;
    int count_y = 0;
    public float[] current;
    public bool done;
    int t;
   public float sum;
    public float average;
    // Use this for initialization



    void Start () {
         count_x = width;
        count_y = height;
     

        kernel = compute.FindKernel("GameOfLife");

        Input = new RenderTexture(width, height, 24);
        Input.name = "Input";
        Input.wrapMode = TextureWrapMode.Repeat;
        Input.enableRandomWrite = true;
        Input.filterMode = FilterMode.Point;
        Input.useMipMap = false;
        Input.Create();

        Output = new RenderTexture(width, height, 24);
        Output.name = "Output";
        Output.wrapMode = TextureWrapMode.Repeat;
        Output.enableRandomWrite = true;
        Output.filterMode = FilterMode.Point;
        Output.useMipMap = false;
        Output.Create();

    

        pingPong = true;

        compute.SetFloat("Width", width);
        compute.SetFloat("Height", height);
        Graphics.Blit(input, Output);
        Graphics.Blit(input, Input);

    }

    // Update is called once per frame
    void Update()
    {

        if (!done)
        {



            compute.SetTexture(kernel, "Input", Input);

            compute.SetTexture(kernel, "Result", Output);
            compute.Dispatch(kernel, width / 1, height / 1, 1);

            material.mainTexture = Output;

            texture = new Texture2D(count_x, count_y, TextureFormat.RGB24, false);

            Rect rectReadPicture = new Rect(0, 0, count_x, count_y);

            RenderTexture.active = Output;

            // Read pixels
            texture.ReadPixels(rectReadPicture, 0, 0);
            texture.Apply();

            RenderTexture.active = null; // added to avoid errors 

            Graphics.Blit(texture, Input);
            current[t] = (1f / Time.unscaledDeltaTime);
            t++;
        }

        if (done)
        {
            if(average == 0)
            {
                for (var i = 0; i < current.Length; i++)
                {
                    sum += current[i];
                }
                average = sum / current.Length;
            }
          
        }
    }
}
