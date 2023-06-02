using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace OceanToolkit
{
    using VH = VectorHelpers;

    [ExecuteInEditMode]
    public class Ocean : MonoBehaviour
    {
        protected static bool DebugMode = false;
        protected static bool WireframeMode = false;
        protected static CameraEvent CommandBufferEvent = CameraEvent.AfterImageEffectsOpaque;
        protected static float MinCameraSurfaceDistance = 10.0f;
        protected static float CameraFocusDistance = 5.0f;

        [SerializeField] protected Material mat;
        [SerializeField] protected Light sun;

        [SerializeField] protected float windAngle = 0.0f;
        [SerializeField] protected Vector4 waveAngles = new Vector4(0.0f, 17.0f, 0.0f, 0.0f);
        [SerializeField] protected Vector4 waveSpeeds = new Vector4(1.0f, 3.0f, 0.0f, 0.0f);
        [SerializeField] protected Vector4 waveScales = new Vector4(0.5f, 2.0f, 0.0f, 0.0f);
        [SerializeField] protected Vector4 waveLengths = new Vector4(8.0f, 30.0f, 10.0f, 10.0f);
        [SerializeField] protected Vector4 waveExponents = new Vector4(1.0f, 4.0f, 1.0f, 1.0f);
        protected Vector4 waveOffsets = Vector4.zero;
        protected Vector4 waveDirection01 = Vector4.zero;
        protected Vector4 waveDirection23 = Vector4.zero;
        protected Vector4 waveConstants = Vector4.zero;
        protected Vector4 waveDerivativeConstants = Vector4.zero;

        [SerializeField] protected float normalMapAngle0 = 0.0f;
        [SerializeField] protected float normalMapAngle1 = 36.0f;
        [SerializeField] protected float normalMapSpeed0 = 0.5f;
        [SerializeField] protected float normalMapSpeed1 = 0.3f;
        protected Vector2 normalMapOffset0 = Vector2.zero;
        protected Vector2 normalMapOffset1 = Vector2.zero;

        [SerializeField] protected float foamMapAngle = 180.0f;
        [SerializeField] protected float foamMapSpeed = 0.05f;
        protected Vector2 foamMapOffset = Vector2.zero;

        [SerializeField] protected int meshResolutionX = 128;
        [SerializeField] protected int meshResolutionY = 128;
        [SerializeField] protected float meshBoundsSize = 10000.0f;
        [SerializeField] protected bool mainCameraOnly = false;
        [SerializeField] protected bool sceneCameraFixFarPlane = true;
        [SerializeField] protected float sceneCameraFarPlane = 1000.0f;

        protected Mesh mesh;
        protected float position;
        protected float positionTop;
        protected float farPlaneDuringRendering;

        public Material OceanMaterial
        {
            get { return mat; }
            set { mat = value; }
        }

        public Light SunLight
        {
            get { return sun; }
            set { sun = value; }
        }

        public float WindAngle
        {
            get { return windAngle; }
            set { windAngle = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        public float WaveAngle0
        {
            get { return waveAngles.x; }
            set { waveAngles.x = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        public float WaveAngle1
        {
            get { return waveAngles.y; }
            set { waveAngles.y = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        public float WaveAngle2
        {
            get { return waveAngles.z; }
            set { waveAngles.z = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        public float WaveAngle3
        {
            get { return waveAngles.w; }
            set { waveAngles.w = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        public float WaveSpeed0
        {
            get { return waveSpeeds.x; }
            set { waveSpeeds.x = Mathf.Max(0.0f, value); }
        }

        public float WaveSpeed1
        {
            get { return waveSpeeds.y; }
            set { waveSpeeds.y = Mathf.Max(0.0f, value); }
        }

        public float WaveSpeed2
        {
            get { return waveSpeeds.z; }
            set { waveSpeeds.z = Mathf.Max(0.0f, value); }
        }

        public float WaveSpeed3
        {
            get { return waveSpeeds.w; }
            set { waveSpeeds.w = Mathf.Max(0.0f, value); }
        }

        public float WaveScale0
        {
            get { return waveScales.x; }
            set { waveScales.x = Mathf.Max(0.0f, value); }
        }

        public float WaveScale1
        {
            get { return waveScales.y; }
            set { waveScales.y = Mathf.Max(0.0f, value); }
        }

        public float WaveScale2
        {
            get { return waveScales.z; }
            set { waveScales.z = Mathf.Max(0.0f, value); }
        }

        public float WaveScale3
        {
            get { return waveScales.w; }
            set { waveScales.w = Mathf.Max(0.0f, value); }
        }

        public float WaveLength0
        {
            get { return waveLengths.x; }
            set { waveLengths.x = Mathf.Max(Mathf.Epsilon, value); }
        }

        public float WaveLength1
        {
            get { return waveLengths.y; }
            set { waveLengths.y = Mathf.Max(Mathf.Epsilon, value); }
        }

        public float WaveLength2
        {
            get { return waveLengths.z; }
            set { waveLengths.z = Mathf.Max(Mathf.Epsilon, value); }
        }

        public float WaveLength3
        {
            get { return waveLengths.w; }
            set { waveLengths.w = Mathf.Max(Mathf.Epsilon, value); }
        }

        public float WaveSharpness0
        {
            get { return waveExponents.x; }
            set { waveExponents.x = Mathf.Max(1.0f, value); }
        }

        public float WaveSharpness1
        {
            get { return waveExponents.y; }
            set { waveExponents.y = Mathf.Max(1.0f, value); }
        }

        public float WaveSharpness2
        {
            get { return waveExponents.z; }
            set { waveExponents.z = Mathf.Max(1.0f, value); }
        }

        public float WaveSharpness3
        {
            get { return waveExponents.w; }
            set { waveExponents.w = Mathf.Max(1.0f, value); }
        }

        public float NormalMapAngle0
        {
            get { return normalMapAngle0; }
            set { normalMapAngle0 = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        public float NormalMapAngle1
        {
            get { return normalMapAngle1; }
            set { normalMapAngle1 = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        public float NormalMapSpeed0
        {
            get { return normalMapSpeed0; }
            set { normalMapSpeed0 = Mathf.Max(0.0f, value); }
        }

        public float NormalMapSpeed1
        {
            get { return normalMapSpeed1; }
            set { normalMapSpeed1 = Mathf.Max(0.0f, value); }
        }

        public float FoamMapAngle
        {
            get { return foamMapAngle; }
            set { foamMapAngle = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        public float FoamMapSpeed
        {
            get { return foamMapSpeed; }
            set { foamMapSpeed = Mathf.Max(0.0f, value); }
        }

        public int ScreenSpaceMeshResolutionX
        {
            get { return meshResolutionX; }
            set
            {
                int clamped = Mathf.Max(1, value);

                if (meshResolutionX != clamped)
                {
                    meshResolutionX = clamped;
                    GenerateQuadMesh();
                }
            }
        }

        public int ScreenSpaceMeshResolutionY
        {
            get { return meshResolutionY; }
            set
            {
                int clamped = Mathf.Max(1, value);

                if (meshResolutionY != clamped)
                {
                    meshResolutionY = clamped;
                    GenerateQuadMesh();
                }
            }
        }

        public float ScreenSpaceMeshBoundsSize
        {
            get { return meshBoundsSize; }
            set
            {
                float clamped = Mathf.Max(0.0f, value);

                if (meshBoundsSize != clamped)
                {
                    meshBoundsSize = clamped;
                    GenerateQuadMesh();
                }
            }
        }

        public bool MainCameraOnly
        {
            get { return mainCameraOnly; }
            set { mainCameraOnly = value; }
        }

        public bool SceneCameraFixFarPlane
        {
            get { return sceneCameraFixFarPlane; }
            set { sceneCameraFixFarPlane = value; }
        }

        public float SceneCameraFarPlane
        {
            get { return sceneCameraFarPlane; }
            set { sceneCameraFarPlane = Mathf.Max(0.0f, value); }
        }

        protected static Mesh CreateQuadMesh(string name, int resolutionX, int resolutionY, float boundsSize)
        {
            int rx = resolutionX + 1;
            int ry = resolutionY + 1;

            Vector3[] vertices = new Vector3[rx * ry];
            int[] indices = new int[(rx - 1) * (ry - 1) * 2 * 3];

            // Place vertices from the top-left corner of the screen, from left to right, row by row
            for (int y = 0; y < ry; y++)
            {
                for (int x = 0; x < rx; x++)
                {
                    vertices[x * ry + y] = new Vector3((float)x / (rx - 1), 1.0f - (float)y / (ry - 1), 0.0f);
                }
            }

            int index = 0;

            for (int y = 0; y < ry - 1; y++)
            {
                for (int x = 0; x < rx - 1; x++)
                {
                    indices[index++] = (x + 0) * ry + (y + 0);
                    indices[index++] = (x + 1) * ry + (y + 1);
                    indices[index++] = (x + 0) * ry + (y + 1);

                    indices[index++] = (x + 0) * ry + (y + 0);
                    indices[index++] = (x + 1) * ry + (y + 0);
                    indices[index++] = (x + 1) * ry + (y + 1);
                }
            }

            return new Mesh
            {
                name = name,
                vertices = vertices,
                triangles = indices,
                bounds = new Bounds(Vector3.zero, Vector3.one * boundsSize)
            };
        }

        protected void GenerateQuadMesh()
        {
            mesh = CreateQuadMesh("Ocean Mesh", meshResolutionX, meshResolutionY, meshBoundsSize);

            if (DebugMode)
            {
                Debug.Log("Ocean mesh generated (" + mesh.vertexCount + " vertices)");
            }
        }

        protected void IntersectFrustumEdgeWaterPlane(Vector3 start, Vector3 end, List<Vector3> outPoints)
        {
            Plane topPlane = new Plane(Vector3.up, Vector3.up * positionTop);
            Plane bottomPlane = new Plane(Vector3.up, Vector3.up * position);

            Vector3 delta = end - start;
            Vector3 direction = delta.normalized;
            float length = delta.magnitude;

            float distance;

            if (topPlane.Raycast(new Ray(start, direction), out distance))
            {
                if (distance <= length)
                {
                    Vector3 hit = start + direction * distance;

                    outPoints.Add(new Vector3(hit.x, position, hit.z));
                }
            }

            if (bottomPlane.Raycast(new Ray(start, direction), out distance))
            {
                if (distance <= length)
                {
                    Vector3 hit = start + direction * distance;

                    outPoints.Add(new Vector3(hit.x, position, hit.z));
                }
            }
        }

        protected void IntersectFrustumWaterPlane(Camera cam, List<Vector3> outPoints)
        {
            var corners = new Vector3[8];

            corners[0] = cam.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, cam.nearClipPlane));
            corners[1] = cam.ViewportToWorldPoint(new Vector3(0.0f, 1.0f, cam.nearClipPlane));
            corners[2] = cam.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, cam.nearClipPlane));
            corners[3] = cam.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, cam.nearClipPlane));

            corners[4] = cam.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, cam.farClipPlane));
            corners[5] = cam.ViewportToWorldPoint(new Vector3(0.0f, 1.0f, cam.farClipPlane));
            corners[6] = cam.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, cam.farClipPlane));
            corners[7] = cam.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, cam.farClipPlane));

            outPoints.Clear();

            foreach (Vector3 corner in corners)
            {
                if (corner.y <= positionTop && corner.y >= position)
                {
                    outPoints.Add(new Vector3(corner.x, position, corner.z));
                }
            }

            IntersectFrustumEdgeWaterPlane(corners[0], corners[1], outPoints);
            IntersectFrustumEdgeWaterPlane(corners[1], corners[2], outPoints);
            IntersectFrustumEdgeWaterPlane(corners[2], corners[3], outPoints);
            IntersectFrustumEdgeWaterPlane(corners[3], corners[0], outPoints);

            IntersectFrustumEdgeWaterPlane(corners[4], corners[5], outPoints);
            IntersectFrustumEdgeWaterPlane(corners[5], corners[6], outPoints);
            IntersectFrustumEdgeWaterPlane(corners[6], corners[7], outPoints);
            IntersectFrustumEdgeWaterPlane(corners[7], corners[4], outPoints);

            IntersectFrustumEdgeWaterPlane(corners[0], corners[4], outPoints);
            IntersectFrustumEdgeWaterPlane(corners[1], corners[5], outPoints);
            IntersectFrustumEdgeWaterPlane(corners[2], corners[6], outPoints);
            IntersectFrustumEdgeWaterPlane(corners[3], corners[7], outPoints);
        }

        protected static Vector3[] ProjectPointsToNdc(Matrix4x4 viewProj, List<Vector3> points)
        {
            var ndcPoints = new Vector3[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                ndcPoints[i] = viewProj.MultiplyPoint(points[i]);
            }

            return ndcPoints;
        }

        protected static Matrix4x4 MapNdcBoundingBoxToFullscreen(Vector3[] ndcPoints)
        {
            // Find ndc bounding box
            Vector3 min = ndcPoints[0];
            Vector3 max = ndcPoints[0];

            for (int i = 1; i < ndcPoints.Length; i++)
            {
                min = Vector3.Min(min, ndcPoints[i]);
                max = Vector3.Max(max, ndcPoints[i]);
            }

            Vector2 size = max - min;

            // Create range matrix so that all points in the bounding box are mapped to [0,1]
            Matrix4x4 range = new Matrix4x4();

            range.m00 = 1.0f / size.x;
            range.m10 = 0.0f;
            range.m20 = 0.0f;
            range.m30 = 0.0f;

            range.m01 = 0.0f;
            range.m11 = 1.0f / size.y;
            range.m21 = 0.0f;
            range.m31 = 0.0f;

            range.m02 = 0.0f;
            range.m12 = 0.0f;
            range.m22 = 1.0f;
            range.m32 = 0.0f;

            range.m03 = -(min.x / size.x);
            range.m13 = -(min.y / size.y);
            range.m23 = 0.0f;
            range.m33 = 1.0f;

            return range;
        }

        protected void UpdateParams()
        {
            position = transform.position.y;
            positionTop = position + VH.Sum(waveScales);

            // Update wave function animation
            Vector4 wAngle = (Vector4.one * windAngle + waveAngles) * Mathf.Deg2Rad;
            waveDirection01 = new Vector4(Mathf.Cos(wAngle.x), Mathf.Sin(wAngle.x), Mathf.Cos(wAngle.y), Mathf.Sin(wAngle.y));
            waveDirection23 = new Vector4(Mathf.Cos(wAngle.z), Mathf.Sin(wAngle.z), Mathf.Cos(wAngle.w), Mathf.Sin(wAngle.w));
            waveOffsets += waveSpeeds * Time.deltaTime;
            waveConstants = VH.Div(Vector4.one * (2.0f * Mathf.PI), waveLengths);
            waveDerivativeConstants = 0.5f * VH.Mul(VH.Mul(waveScales, waveConstants), waveExponents);

            // Update texture animations
            float nAngle0 = (windAngle + normalMapAngle0) * Mathf.Deg2Rad;
            float nAngle1 = (windAngle + normalMapAngle1) * Mathf.Deg2Rad;
            normalMapOffset0 += new Vector2(Mathf.Cos(nAngle0), Mathf.Sin(nAngle0)) * normalMapSpeed0 * Time.deltaTime;
            normalMapOffset1 += new Vector2(Mathf.Cos(nAngle1), Mathf.Sin(nAngle1)) * normalMapSpeed1 * Time.deltaTime;

            float fAngle = (windAngle + foamMapAngle) * Mathf.Deg2Rad;
            foamMapOffset += new Vector2(Mathf.Cos(fAngle), Mathf.Sin(fAngle)) * foamMapSpeed * Time.deltaTime;

            // Reset if in editor
            if (!Application.isPlaying)
            {
                waveOffsets = Vector4.zero;
                normalMapOffset0 = Vector4.zero;
                normalMapOffset1 = Vector4.zero;
                foamMapOffset = Vector4.zero;
            }
        }

        protected void SendParamsToMaterial()
        {
            if (mat == null || !mat.HasProperty("ot_NormalMap0"))
            {
                return;
            }

            mat.SetFloat("ot_OceanHeight", position);

            // Wave function animation
            mat.SetVector("ot_WaveScales", waveScales);
            mat.SetVector("ot_WaveLengths", waveLengths);
            mat.SetVector("ot_WaveExponents", waveExponents);
            mat.SetVector("ot_WaveOffsets", waveOffsets);
            mat.SetVector("ot_WaveDirection01", waveDirection01);
            mat.SetVector("ot_WaveDirection23", waveDirection23);
            mat.SetVector("ot_WaveConstants", waveConstants);
            mat.SetVector("ot_WaveDerivativeConstants", waveDerivativeConstants);

            // Texture animations
            Vector2 normalMapScale0 = mat.GetTextureScale("ot_NormalMap0");
            Vector2 normalMapScale1 = mat.GetTextureScale("ot_NormalMap1");
            Vector2 foamMapScale = mat.GetTextureScale("ot_FoamMap");

            mat.SetTextureOffset("ot_NormalMap0", VH.Mul(normalMapOffset0, normalMapScale0));
            mat.SetTextureOffset("ot_NormalMap1", VH.Mul(normalMapOffset1, normalMapScale1));
            mat.SetTextureOffset("ot_FoamMap", VH.Mul(foamMapOffset, foamMapScale));

            // General
            Vector3 lightDir = Vector3.up;

            if (sun != null)
            {
                lightDir = -sun.transform.forward;
            }

            mat.SetVector("ot_LightDir", lightDir);

            float dwIntensityZenith = mat.GetFloat("ot_DeepWaterIntensityZenith");
            float dwIntensityHorizon = mat.GetFloat("ot_DeepWaterIntensityHorizon");
            float dwIntensityDark = mat.GetFloat("ot_DeepWaterIntensityDark");

            float dwScalar = 0.0f;

            if (lightDir.y >= 0.0f)
            {
                dwScalar = Mathf.Lerp(dwIntensityHorizon, dwIntensityZenith, lightDir.y);
            }
            else
            {
                dwScalar = Mathf.Lerp(dwIntensityHorizon, dwIntensityDark, -lightDir.y);
            }

            mat.SetFloat("ot_DeepWaterScalar", dwScalar);
        }

        protected List<Vector3> frustumWaterIntersectionPoints = new List<Vector3>();

        protected void PreRender(Camera cam)
        {
            if (WireframeMode)
            {
                GL.wireframe = true;
            }

            farPlaneDuringRendering = cam.farClipPlane;

            if (cam.cameraType == CameraType.SceneView && sceneCameraFixFarPlane)
            {
                cam.farClipPlane = sceneCameraFarPlane;
            }

            if (mainCameraOnly && cam != Camera.main)
            {
                return;
            }

            if (mat == null)
            {
                return;
            }

            IntersectFrustumWaterPlane(cam, frustumWaterIntersectionPoints);

            // Does the view frustum intersect the ocean plane?
            if (frustumWaterIntersectionPoints.Count > 0)
            {
                Plane waterPlane = new Plane(Vector3.up, Vector3.up * position);

                // Set up new view and projection matrices where the camera/projector position is always above the water
                Vector3 camPos = cam.transform.position;
                camPos.y = Mathf.Max(camPos.y, positionTop + MinCameraSurfaceDistance);

                // It is important that the focus point is below camPos at all times!
                Vector3 focus = cam.transform.position + cam.transform.forward * CameraFocusDistance;

                if (DebugMode)
                {
                    Debug.DrawLine(camPos, focus, Color.white);
                }

                // Construct view frame
                Vector3 viewFrameZ = Vector3.Normalize(focus - camPos);
                Vector3 viewFrameX = Vector3.Cross(Vector3.up, viewFrameZ).normalized;
                Vector3 viewFrameY = Vector3.Cross(viewFrameZ, viewFrameX).normalized;

                Matrix4x4 viewFrame = new Matrix4x4();
                viewFrame.SetColumn(0, viewFrameX);
                viewFrame.SetColumn(1, viewFrameY);
                viewFrame.SetColumn(2, -viewFrameZ);
                viewFrame.SetColumn(3, new Vector4(camPos.x, camPos.y, camPos.z, 1.0f));

                // Construct view and projection matrices
                Matrix4x4 view = viewFrame.inverse;
                Matrix4x4 proj = cam.projectionMatrix;
                Matrix4x4 viewProj = proj * view;

                mat.SetMatrix("ot_Proj", proj);
                mat.SetMatrix("ot_InvView", view.inverse);

                // Project the intersection points of the frustum and water plane into ndc-space, as seen from the camera
                Vector3[] ndcPoints = ProjectPointsToNdc(viewProj, frustumWaterIntersectionPoints);

                // Create a matrix that maps ndc-points that are within the bounding box of the water plane in (ndc-space) to [0,1]
                Matrix4x4 rangeMap = MapNdcBoundingBoxToFullscreen(ndcPoints);

                // The projector transform is from world-space to remapped ndc-space where only ndc-points inside the bounding box of the water plane lies within [0,1]
                Matrix4x4 toProjectorSpace = rangeMap * viewProj;
                Matrix4x4 fromProjectorSpace = toProjectorSpace.inverse;

                // Find out where the corners of the bounding box of the water plane intersect the water plane
                Vector2[] corners = {   new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f),
                                        new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f) };
                Color[] colors = { Color.red, Color.green, Color.blue, Color.black };

                Vector4[] qkCorner = new Vector4[4];
                for (int i = 0; i < corners.Length; i++)
                {
                    Vector2 corner = corners[i];

                    Vector3 near = new Vector3(corner.x, corner.y, -1.0f);
                    Vector3 far = new Vector3(corner.x, corner.y, 1.0f);
                    Vector3 start = fromProjectorSpace.MultiplyPoint(near);
                    Vector3 end = fromProjectorSpace.MultiplyPoint(far);
                    Vector3 dir = (end - start).normalized;

                    float d;
                    waterPlane.Raycast(new Ray(start, dir), out d);
                    Vector3 hit = start + dir * d;

                    if (DebugMode)
                    {
                        if (d > 0.0f)
                        {
                            Debug.DrawRay(start, hit - start, colors[i]);
                        }
                    }

                    // Divide view space coordinate by projection w
                    Vector4 viewCorner = view * new Vector4(hit.x, hit.y, hit.z, 1.0f);
                    viewCorner = viewCorner / viewCorner.w;
                    Vector4 projCorner = proj * viewCorner;
                    qkCorner[i] = viewCorner / projCorner.w;
                }

                // Send corners to mat
                mat.SetVector("ot_QkCorner0", qkCorner[0]);
                mat.SetVector("ot_QkCorner1", qkCorner[1]);
                mat.SetVector("ot_QkCorner2", qkCorner[2]);
                mat.SetVector("ot_QkCorner3", qkCorner[3]);
            }
        }

        protected void PostRender(Camera cam)
        {
            GL.wireframe = false;

            if (cam.cameraType == CameraType.SceneView)
            {
                cam.farClipPlane = farPlaneDuringRendering;
            }
        }

        public float GetHeightAt(Vector3 point)
        {
            Vector2 xz = VH.GetXZ(point);
            Vector4 locations = new Vector4(Vector2.Dot(VH.GetXY(waveDirection01), xz), Vector2.Dot(VH.GetZW(waveDirection01), xz), Vector2.Dot(VH.GetXY(waveDirection23), xz), Vector2.Dot(VH.GetZW(waveDirection23), xz));
            Vector4 sine = VH.Sin(VH.Mul((locations + waveOffsets), waveConstants)) * 0.5f + new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
            float sum = Vector4.Dot(waveScales, VH.Pow(sine, waveExponents));
            return position + sum;
        }

        public void OnEnable()
        {
            Camera.onPreCull += PreRender;
            Camera.onPostRender += PostRender;
        }

        public void OnDisable()
        {
            Camera.onPreCull -= PreRender;
            Camera.onPostRender -= PostRender;
        }

        public void Start()
        {
            if (mesh == null || mesh.vertexCount != (meshResolutionX + 1) * (meshResolutionY + 1))
            {
                GenerateQuadMesh();
            }
        }

        public void Update()
        {
            UpdateParams();

            SendParamsToMaterial();

            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, mat, gameObject.layer);
        }

        public void OnDrawGizmos()
        {
            if (DebugMode)
            {
                if (Camera.main == null)
                {
                    return;
                }

                var points = new List<Vector3>();
                IntersectFrustumWaterPlane(Camera.main, points);

                Gizmos.color = Color.red;

                foreach (Vector3 p in points)
                {
                    Gizmos.DrawSphere(p, 0.25f);
                }
            }
        }
    }
}
