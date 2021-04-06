using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;

public class NetRunnerBehavior : MonoBehaviour
{
    public NNModel modelSource;
    private Model mRuntimeModel;
    private IWorker mWorker;
    public Texture2D[] mNetInput;
    public GameObject displayObject;
    public GameObject displayFlatObject;
    //private Dictionary<string, NetLayer> layerMap;
    private ArrayList layers;
    private string[] mOutputs;
    public GameObject layerParticleSystemPrefab;
    public GameObject particleLightPrefab;
    public GameObject unitText;
    private ArrayList layerPSInstances;
    private ArrayList layerActiveParticles;
    private ArrayList particleLights;
    private ArrayList layerEdges;
    ParticleSystem.Particle[] cloud;
    public int curNetInputIdx;
    public float updateInterval;
    public Texture enableLineTexture;
    public Texture disableLineTexture;
    private float lastUpdateInterval;
    private GameObject camObject;
    private Camera mainCam;
    private GameObject displayCubesObject;
    private bool update;
    private int hitIndex;
    private int particleIndex;
    private bool animEnabled;
    public Button animToggleButton;
    // Start is called before the first frame update
    void Start()
    {
        camObject = GameObject.Find("Main Camera");
        displayCubesObject = GameObject.Find("DisplayCubes");
        unitText = GameObject.Find("UnitText");
        mainCam = camObject.GetComponent<Camera>();
        mOutputs = new string[] { "dense_7/Relu:0", "softmax_2" };
        mRuntimeModel = ModelLoader.Load(modelSource);
        mWorker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharp, mRuntimeModel, mOutputs,false);
        //layerMap = new Dictionary<string, NetLayer>();
        layers = new ArrayList();
        layerPSInstances = new ArrayList();
        layerActiveParticles = new ArrayList();
        particleLights = new ArrayList();
        layerEdges = new ArrayList();
        initLayers();
        //curNetInputIdx = 5;
        lastUpdateInterval = Time.time;
        hitIndex = -1;
        particleIndex = -1;
        animEnabled = true;
        
        /*float v = 5.0f;
        cloud = new ParticleSystem.Particle[4];
        cloud[0].position = new Vector3(v, v, v);
        cloud[1].position = new Vector3(-v, v, -v);
        cloud[2].position = new Vector3(-v, v, v);
        cloud[3].position = new Vector3(v, v, -v);

        for (int ii = 0; ii < cloud.Length; ++ii)
        {
            ParticleSystem.Particle p = cloud[ii];
            p.color = Color.red;
            p.velocity = Vector3.zero;
            p.angularVelocity = 0.0f;
            p.rotation = 0.0f;
            p.size = 0.1f;
            p.remainingLifetime = 100000f;
            //p.remainingLifetime = 100.0f;
            p.randomValue = 0.0f;
        }
        var emitparams = new ParticleSystem.EmitParams();
        for (int inc2 = 0; inc2 < cloud.Length; inc2++)
        {
            emitparams.position = cloud[inc2].position;
            GetComponent<ParticleSystem>().Emit(emitparams, cloud.Length);
        }
        //GetComponent<ParticleSystem>().SetParticles(cloud, cloud.Length);
        //GetComponent<ParticleSystem>().Play();
        */
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        
        //if (Input.GetMouseButtonDown(0))
        {
            hitIndex = GetHitParticle(Input.mousePosition);
        }
        if (Time.time - lastUpdateInterval >= updateInterval)
        {
            var channelCount = 1;
            var input = new Tensor(mNetInput[curNetInputIdx], channelCount);
            //Debug.Log("predicted:" + input.AsFloats());
            mWorker.Execute(input);

            for (int i = 0; i < mOutputs.Length; i++)
            {
                Tensor output = mWorker.PeekOutput(mOutputs[i]);

                float[] val = output.readonlyArray;
                var layerPSInstance = layerPSInstances[i] as GameObject;
                var activeParticles = layerActiveParticles[i] as ParticleSystem.Particle[];
                int numParticlesAlive = layerPSInstance.GetComponent<ParticleSystem>().GetParticles(activeParticles);
                var activeLights = particleLights[i] as GameObject[];
                
                for (int cnt = 0; cnt < val.Length; cnt++)
                {
                    var lightObj = activeLights[cnt] as GameObject;
                    if (hitIndex != -1 && hitIndex < val.Length && particleIndex == i)
                    {
                        unitText.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].text = "Unit:" +
                            mOutputs[i] + "-" + hitIndex.ToString() + " \n " + val[hitIndex].ToString();
                    }
                    /* 
                     float spread = 0.005f;
                     var pos = activeParticles[cnt].position;
                     float offsetX = Random.Range(-spread, spread);
                     float offsetY = Random.Range(-spread, spread);
                     float sign = Random.value > 0.5f ? 1.0f:-1.0f;

                     activeParticles[cnt].position = new Vector3(pos.x + sign*offsetX, pos.y + sign*offsetY, pos.z);
                    */
                    if (i == mOutputs.Length - 1)
                    {
                        if (val[cnt] > 0.4f)
                        {
                            activeParticles[cnt].color = Color.green;
                            lightObj.active = true;
                            
                            //ArrayList targetList = new ArrayList();
                            //if (layerEdges.TryGetValue(activeParticles[cnt].GetHashCode(), out targetList))
                            //    updateEdge(layerEdges[activeParticles[cnt].GetHashCode()], Color.cyan);
                        }
                        else
                        {
                            activeParticles[cnt].startColor = Color.gray;
                            lightObj.active = false;
                        }


                    }
                    else
                    {
                        var edges = layerEdges[cnt] as ArrayList;
                        if (val[cnt] > 0.0f)
                        {
                            
                            for (int j = 0; j < edges.Count; j++)
                            {
                                var edgeObj = edges[j] as GameObject;
                                edgeObj.SetActive(true);
                                //Debug.Log("Enabling:" + edgeObj.name);
                                if (edgeObj.name == "Edge_" + activeParticles[cnt].position)
                                {
                                    var lineRenderer = edgeObj.GetComponent<LineRenderer>();
                                    SetLineFradient(lineRenderer, Color.cyan, Color.green, 0.1f);
                                    //edgeObj.GetComponent<Renderer>().material.SetTexture("_MainTex", enableLineTexture);
                                }
                                    
                            }
                            activeParticles[cnt].startColor = Color.cyan;
                            lightObj.active = true;
                           
                            //lightObj.GetComponent<Light>().intensity *= val[cnt];
                            //ArrayList targetList = new ArrayList();
                            //if (layerEdges.TryGetValue(activeParticles[cnt].GetHashCode(),out targetList))
                            //    updateEdge(layerEdges[activeParticles[cnt].GetHashCode()], Color.cyan);
                        }
                        else
                        {
                            activeParticles[cnt].startColor = Color.gray;
                            lightObj.active = false;
                            for (int j = 0; j < edges.Count; j++)
                            {
                                var edgeObj = edges[j] as GameObject;
                                edgeObj.SetActive(false);
                                if (edgeObj.name == "Edge_" + activeParticles[cnt].position)
                                {
                                    var lineRenderer = edgeObj.GetComponent<LineRenderer>();
                                    SetLineFradient(lineRenderer, Color.gray, Color.gray, 0.1f);
                                    //edgeObj.GetComponent<Renderer>().material.SetTexture("_MainTex", disableLineTexture);
                                }

                            }
                        }
                       
                    }
                    var transform = camObject.transform;
                    activeParticles[cnt].rotation3D = new Vector3(360, 360, 360) - transform.localRotation.eulerAngles;

                }
                layerPSInstance.GetComponent<ParticleSystem>().SetParticles(activeParticles, numParticlesAlive);
            }

            displayObject.GetComponent<MeshRenderer>().material.mainTexture = mNetInput[curNetInputIdx];
            displayCubesObject.GetComponent<DisplayCubesCreator>().isTextureUpdated = true;
            displayCubesObject.GetComponent<DisplayCubesCreator>().colorTexture = mNetInput[curNetInputIdx];
            displayFlatObject.GetComponent<FlattenTexture>().srcTexture = mNetInput[curNetInputIdx];
            displayFlatObject.GetComponent<FlattenTexture>().isChanged = true;
            input.Dispose();
            if (animEnabled && ++curNetInputIdx >= mNetInput.Length)
                curNetInputIdx = 0;
            lastUpdateInterval = Time.time;
        }
        
    }
    long getMillis()
    {
       return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
    }


    private void initLayers()
    {
        var channelCount = 1;
        var input = new Tensor(mNetInput[curNetInputIdx], channelCount);
        //Debug.Log("predicted:" + input.AsFloats());
        mWorker.Execute(input);
        float centerPos = gameObject.transform.position.z;
        for (int i=0;i< mOutputs.Length; i++)
        {
            Tensor output = mWorker.PeekOutput(mOutputs[i]);
            drawLayer(output.name, output, centerPos, 1.0f);
            centerPos += -0.5f;
        }
        drawInputEdges(0, new float[] { 3, 3 });
        //draw connectors
        for (int i = 1; i < layers.Count; i++)
        {
            //i-1 -> i
            drawEdges(i - 1, i);
        }
        input.Dispose();
    }

    private void drawLayer(string type,Tensor output,float centerPos, float spread)
    {
        var layer = new NetLayer(type, output.ToReadOnlyArray(), centerPos, spread);
        layers.Add(layer);
        //var lineRenderer = gameObject.AddComponent<LineRenderer>();
        Vector3 startPos = new Vector3(0.0f, 0.3f, centerPos);

        //Vector3 endPos = new Vector3(startPos.x, startPos.y, startPos.z);
        var layerPSInstance = Instantiate(layerParticleSystemPrefab, startPos, Quaternion.AngleAxis(-90, new Vector3(1.0f,0.0f,0.0f)));
        var layerText = layerPSInstance.GetComponentsInChildren<TMPro.TextMeshPro>()[0];
       
        if (layerText != null)
            layerText.text = type;
        var particleSystem = layerPSInstance.GetComponent<ParticleSystem>();
        //particleSystem.Stop();
        var main = particleSystem.main;
        main.maxParticles = output.length;
        //layerPSInstance.GetComponent<ParticleSystem>().shape.scale = new Vector3(1 / data.Length, 1 / data.Length, 0.1f);
        var activeParticles = new ParticleSystem.Particle[layerPSInstance.GetComponent<ParticleSystem>().main.maxParticles];
        int numParticlesAlive = layerPSInstance.GetComponent<ParticleSystem>().GetParticles(activeParticles);
        var lights = new GameObject[activeParticles.Length];
        for (int i = 0; i < activeParticles.Length  ; i++)
        {
            //activeParticles[i].color = Color.red;
            activeParticles[i].velocity = Vector3.zero;
            activeParticles[i].angularVelocity = 0.0f;
            activeParticles[i].rotation = 0.0f;
            //activeParticles[i].size = 0.1f;
            lights[i] = Instantiate(particleLightPrefab);
            activeParticles[i].remainingLifetime = 100000f;
            //p.remainingLifetime = 100.0f;
            //activeParticles[i].randomValue = 0.0f;
            float offset = 0.05f;
            float offsetX = Random.Range(-spread/4, spread/4);
            float offsetY = Random.Range(0, spread/2);
            float offsetZ = Random.Range(0, spread/8);
            float sign = Random.value > 0.5f ? 1.0f : -1.0f;
            if (!type.Contains("softmax"))
            {
                activeParticles[i].position = new Vector3(startPos.x + offsetX,  offsetY, startPos.z - 0.001f);
            }
            else
            {
                activeParticles[i].position = new Vector3(startPos.x + offset*i - 0.2f, startPos.y, startPos.z-0.001f );
            }
            lights[i].transform.position = activeParticles[i].position - new Vector3(0,0,0.001f);
            activeParticles[i].remainingLifetime = 10000f;
            var emitparams = new ParticleSystem.EmitParams();
            emitparams.position = activeParticles[i].position;
            
            particleSystem.Emit(emitparams, 1);
            
            if (!type.Contains("softmax"))
            {
                lights[i].GetComponent<Light>().color = Color.cyan;
            }
            else
                lights[i].GetComponent<Light>().color = Color.red;
        }
        
        //var shape = particleSystem.shape;
        //shape.shapeType = ParticleSystemShapeType.Rectangle;
        //shape.texture = tex;

        //particleSystem.SetParticles(activeParticles,numParticlesAlive);
        //particleSystem.Play();
        //Debug.Log(numParticlesAlive);
        layerPSInstances.Add(layerPSInstance);
        layerActiveParticles.Add(activeParticles);
        particleLights.Add(lights);
        //lineRenderer.positionCount = 2;
        //lineRenderer.SetPositions(positions);
        //lineRenderer.startWidth = 0.3f;
        //lineRenderer.enabled = true;
    }

    private void drawEdges(int layer1Idx, int layer2Idx)
    {
        var layer1 = layers[layer1Idx] as NetLayer;
        var layer2 = layers[layer2Idx] as NetLayer;
        var activeParticles1 = layerActiveParticles[layer1Idx] as ParticleSystem.Particle[];
        var activeParticles2 = layerActiveParticles[layer2Idx] as ParticleSystem.Particle[];
        if (activeParticles1 != null && activeParticles2 != null)
        {
            for(int i=0;i<activeParticles1.Length;i++)
            {
                var targetList = new ArrayList();
                for (int j=0;j< activeParticles2.Length;j++)
                {
                    var edge = drawLine(activeParticles1[i].position, activeParticles2[j].position, new Color(255.0f, 255.0f, 255.0f, 0.1f),true);
                    targetList.Add(edge);
                }
                layerEdges.Add(targetList);
            }
        }
        
    }

    private void drawInputEdges(int firstHiddenIdx, float[] inputShape)
    {
        /*Vector3[] vertices = displayFlatObject.GetComponent<MeshFilter>().mesh.vertices;
        // displayObject.transform.
        float minX =  100000f;
        float minY =  100000f;
        float minZ =  100000f;
        float maxX =  -100000f;
        float maxY =  -100000f;
        float maxZ =  -100000f;
       for(int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].x < minX)
                minX = vertices[i].x;
            if (vertices[i].y < minY)
                minY = vertices[i].y;
            if (vertices[i].z < minZ)
                minZ = vertices[i].z;
            if (vertices[i].x > maxX)
                maxX = vertices[i].x;
            if (vertices[i].y > maxY)
                maxY = vertices[i].y;
            if (vertices[i].z > maxZ)
                maxZ = vertices[i].z;
        }
        var firstHiddenLayer = layerActiveParticles[firstHiddenIdx] as ParticleSystem.Particle[];
       
        Debug.Log("minX:"+minX +" minY:"+minY + " minZ:" + minZ);
        Debug.Log("maxX:" + maxX + " maxY:" + maxY + " maxZ:" + maxZ);
        //Vector3 minVec = new Vector3(minX, minY, maxZ);
        //Vector3 maxVec = new Vector3(maxX, maxY, maxZ);
        Vector3 curVec = new Vector3();
        curVec.x = minX; curVec.y = minY; curVec.z = minZ;
        var offsetX = Mathf.Abs(maxX - minX) / inputShape[0];
        var offsetY = Mathf.Abs(maxY - minY) / inputShape[1];
        curVec.y += offsetY / 2;
        Debug.Log("offsetX:" + offsetX + " offsetY:" + offsetY);
        for (int i = 0; i < inputShape[0]; i++)
        {
            for (int j = 0; j < inputShape[0]; j++)
            {
                for (int k = 0; k < firstHiddenLayer.Length; k++)
                {
                    drawLine(displayFlatObject.transform.TransformPoint(curVec), firstHiddenLayer[k].position, new Color(255.0f,255.0f,255.0f,0.8f));
                }
                //curVec.y += offsetY;
            }
            //curVec.y = minY;
            curVec.x += offsetX;
        }
        */
        var displayCubeCreator = displayCubesObject.GetComponent<DisplayCubesCreator>();
        displayCubeCreator.initMeshes();
        
        var firstHiddenLayer = layerActiveParticles[firstHiddenIdx] as ParticleSystem.Particle[];
        for (int cnt= 0;cnt < displayCubeCreator.meshes.Count;cnt+=(int)Random.Range(4,16))
        {
            var startPos = (displayCubeCreator.meshes[cnt] as GameObject).transform.position;
            for(int i=0;i<firstHiddenLayer.Length;i+=8)
            {
                drawLine(startPos, firstHiddenLayer[i].position, new Color(50f, 50f, 255.0f, 0.2f),true);
            }
        }
    }

    private GameObject drawLine(Vector3 startPos, Vector3 endPos,Color color,bool startActive=false)
    {
        var lineObject = new GameObject("Edge_" + startPos.ToString());
        var edgeParent = GameObject.Find("Edges");
        if (edgeParent != null)
            lineObject.transform.SetParent(edgeParent.transform);
        var line = lineObject.AddComponent<LineRenderer>();
        //line.positionCount = 2;
        //line.SetPositions(new Vector3[] { startPos, endPos });
        Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f) + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f,0.2f),0);
        DrawQuadraticBezierCurve(line, startPos, midPoint, endPos);
        line.startWidth = 0.005f;
        line.endWidth = 0.005f;
  
        var mat = Resources.Load("LineMat") as Material;

        //mat.SetColor("Base Color", color);
        line.material = mat;
        //line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(1,1,1,0.1f);
        line.endColor = new Color(1, 1, 1, 0.1f);
        lineObject.SetActive(startActive);
        return lineObject;
    }
    void DrawQuadraticBezierCurve(LineRenderer lineRenderer, Vector3 point0, Vector3 point1, Vector3 point2)
    {
        lineRenderer.positionCount = 20;
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point1 + t * t * point2;
            lineRenderer.SetPosition(i, B);
            t += (1 / (float)lineRenderer.positionCount);
        }
    }

    private void updateEdge(ArrayList targetList, Color newColor)
    {
        foreach(var target in targetList)
        {
            GameObject targetObj = target as GameObject;
            var lineRenderer = targetObj.GetComponent<LineRenderer>();
            lineRenderer.material.color = newColor;
        }
    }

    private int GetHitParticle(Vector3 position)
    {
        ParticleSystem.Particle? closestHitParticle = null;
        var closestHitDist = float.MaxValue;
       
        for (int i=0;i<layerActiveParticles.Count;i++)
        {
            var layerInstance = layerPSInstances[i] as GameObject;
            var particleSystem = layerInstance.GetComponent<ParticleSystem>();
            var activeParticles = layerActiveParticles[i] as ParticleSystem.Particle[];
            for(int j=0;j<activeParticles.Length;j++)
            {
                var pos = activeParticles[j].position;
                var size = activeParticles[j].GetCurrentSize(particleSystem);
                var distance = Vector3.Distance(pos, mainCam.transform.position);
                var screenSize = angularSizeOnScreen(size, distance, mainCam);
                var screenPos = mainCam.WorldToScreenPoint(pos);
                var screenRect = new Rect(screenPos.x - screenSize / 2, screenPos.y - screenSize / 2, screenSize, screenSize);
                if (screenRect.Contains(Input.mousePosition) && distance < closestHitDist)
                {
                    closestHitParticle = activeParticles[j];
                    closestHitDist = distance;
                    hitIndex = j;
                    particleIndex = i;
                }
            }
        }
        if (closestHitDist < float.MaxValue)
        {
            //Debug.Log($"Hit particle at {closestHitParticle?.position} : {particleIndex.ToString()} : {hitIndex.ToString()}");

        }
        return hitIndex;
    }
    private float angularSizeOnScreen(float diam, float dist, Camera cam)
    {
        var aSize = (diam / dist) * Mathf.Rad2Deg;
        var pSize = ((aSize * Screen.height) / cam.fieldOfView);
        return pSize;
    }

    void SetLineFradient(LineRenderer lr,Color start,Color end, float alpha=1.0f,float time=1.0f)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(start, 0.0f), new GradientColorKey(end, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lr.colorGradient = gradient;
    }

    public void onCameraRotateOptionChanged(int option)
    {
        Debug.Log($"option:{option}");
        if(option == 0)
        {
            camObject.GetComponent<UnityTemplateProjects.SimpleCameraController>().enabled = true;
            camObject.GetComponent<CameraRotationScript>().enabled = false;
        }
        else if(option == 1)
        {
            camObject.GetComponent<UnityTemplateProjects.SimpleCameraController>().enabled = false;
            camObject.GetComponent<CameraRotationScript>().enabled = true;
        }
    }
    public void onNetRunnerAnimEnabled()
    {
        animEnabled = !animEnabled;
        var animToggle = GameObject.Find("Toggle Animation");
       /* if (animEnabled)
           animToggleButton.GetComponent<Text>().text = "Enable Anim";
        else
            animToggleButton.GetComponent<Text>().text = "Disable Anim";*/

    }
}
