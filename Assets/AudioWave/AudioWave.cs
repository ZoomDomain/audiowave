/*

AudioWave display by Tony Stewart.
This code is provided as shareware, 
if it solves your audiowave display tasks
please send 5-10 euros to sr8@live.co.uk
so that i will be able to buy Unity:P

Audio Wave visualization of entire audio songs 
can handle audio files of 100,000,000 samples long, 
audio file peaks are smoothed by finding the max of 
peaks every 4 samples, the audio is averaged to monophonic.

Bit reduced audio is sent to graphics card in the start function
where it is graphed, resized and zoomed fast using compute buffers.

Code is best used on the main camera for the moment, 
there are workarounds to use them on other objects 
except i havent studied that trick.

to used the two codes, make a material, set the shader onto the material, 
put the script onto the main camera, place the material on it. 

for the moment it uses unity's audio PCM Wave API with many options,
for many audio files it can be easier to use other API's like C# stream 
wave decoding API.

There is aliasing if you zoom out and then pan through the audio,
I will upload this file until i have an update with more 
precise control of the ratio in between the pixels and zoom divisions.
for the moment it works very well as a fast view at high detail view.


*/

using UnityEngine;

public class AudioWave : MonoBehaviour {
	
	private float[] samples;
	
	public Material material;
	
	public int offset = 5000;
	public int skipn  = 1;

	public int instanceCount = 1000;
	public int listCount = 10000000;
	
	private ComputeBuffer bufferPoints;
	private ComputeBuffer bufferPos;
	private ComputeBuffer bufferlist;
	private Vector3[] origPos;
	private Vector3[] pos;
	private float[] list;
	
	void Start () {
	
	//get audio data and find Max of every 2 samples from both channels (average 4)
		AudioSource aud = GetComponent<AudioSource>();
        float[] samples = new float[aud.clip.samples * aud.clip.channels];
        aud.clip.GetData(samples, 0);
		print(samples.Length);
		
		var AveragedSound = new float[samples.Length/4];
		var j = 0;
		for (var i = 0; i < samples.Length/4-20; ++i)
		{
		j+=4;
		if (j>=samples.Length)print(j+" er "+ i);
		float average = Mathf.Max(Mathf.Abs(samples[j]),Mathf.Abs(samples[j+1])); 
		average = Mathf.Max(average,Mathf.Abs(samples[j+2])); 
		average = Mathf.Max(average,Mathf.Abs(samples[j+3])); 
		AveragedSound[i] = average*2;

		}
	
	
	//make verts
		var verts = new Vector3[3];
		for (var i = 0; i < 3; ++i)
		{
			float phi = i * Mathf.PI * 2.0f / (3-1);
			verts[i] = new Vector3(0, Mathf.Cos(phi), 0.0f);
		}
		
	//make complex array
		var pos = new Vector3[instanceCount];
		for (var i = 0; i < instanceCount; ++i)
		{
			pos[i] =  new Vector3(
			i* 0.03f,//xdistance
			0,//amplitude
			0
			);
		}
		
		
	//make list array
		var list = AveragedSound;
		for (var i = 0; i < AveragedSound.Length; ++i)
		{
			list[i] = AveragedSound[i];
		}
	
	
		ReleaseBuffers ();
		
		bufferPoints = new ComputeBuffer (3, 12);
		bufferPoints.SetData (verts);
		material.SetBuffer ("buf_Points", bufferPoints);

		bufferPos = new ComputeBuffer (instanceCount, 12);
		bufferPos.SetData (pos);
		material.SetBuffer ("buf_Positions", bufferPos);
		
		bufferlist = new ComputeBuffer (listCount, 12);
		bufferlist.SetData (list);
		material.SetBuffer ("buf_list", bufferlist);
	}
	
	private void ReleaseBuffers() {
		if (bufferPoints != null) bufferPoints.Release();
		bufferPoints = null;
		if (bufferPos != null) bufferPos.Release();
		bufferPos = null;
		if (bufferlist != null) bufferlist.Release();
		bufferlist = null;
	}
	
	private void GetAudioData(){

	
	}
	
	void OnDisable() {
		ReleaseBuffers ();
	}
	
	// each frame, update the positions buffer (one vector per instance)
	void Update () {

		material.SetInt("offset", offset);
		material.SetInt("skipn", skipn);
		
		if (Input.GetMouseButton(0))
            offset+=5+skipn;
        
        if (Input.GetMouseButton(1))
            offset-=5+skipn;
        
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
			skipn*=2;
			
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
			skipn/=2;
			
		if (skipn<1) skipn =1;
			
	}
	
	void OnGUI() {
        GUI.Label(new Rect(10, 10, 1000, 20), "Press mouse buttons to change offset, mouse wheel to skip n samples ");
    }
	// called if script attached to the camera, after all regular rendering is done
	void OnPostRender () {
		material.SetPass (0);
		Graphics.DrawProcedural (MeshTopology.LineStrip, 3, instanceCount);
	}
}