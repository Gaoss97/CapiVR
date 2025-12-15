using System.Collections.Generic;
using UnityEngine;

public class HandInteractionAttach : MonoBehaviour
{
    [Header("MediaPipe Points")]
    public Transform[] points; // 21 pontos do MediaPipe

    [Header("Line Settings")]
    public Material lineMaterial;
    public float lineWidth = 0.01f;

    [Header("Collider Settings")]
    public float colliderThickness = 0.02f;
    public LayerMask grabbableLayer;

    [Header("Pinch Settings")]
    public float pinchThreshold = 0.03f; // distância para detectar pinch
    public float grabRadius = 0.05f;

    private List<Segment> segments = new List<Segment>();
    private bool wasPinching = false;
    private Rigidbody grabbedObject;

    // Conexões do MediaPipe Hand (polegar, dedos)
    private int[][] handConnections = new int[][]
    {
        new int[]{0,1}, new int[]{1,2}, new int[]{2,3}, new int[]{3,4},       // polegar
        new int[]{0,5}, new int[]{5,6}, new int[]{6,7}, new int[]{7,8},       // indicador
        new int[]{0,9}, new int[]{9,10}, new int[]{10,11}, new int[]{11,12},  // médio
        new int[]{0,13}, new int[]{13,14}, new int[]{14,15}, new int[]{15,16},// anelar
        new int[]{0,17}, new int[]{17,18}, new int[]{18,19}, new int[]{19,20} // mindinho
    };

    private class Segment
    {
        public LineRenderer lr;
        public BoxCollider col;
        public Transform a, b;
    }

    void Start()
    {
        // Cria segmentos com LineRenderer + BoxCollider
        foreach (var c in handConnections)
        {
            Segment s = new Segment();
            s.a = points[c[0]];
            s.b = points[c[1]];

            GameObject lineObj = new GameObject("Segment");
            lineObj.transform.parent = transform;

            // LineRenderer
            s.lr = lineObj.AddComponent<LineRenderer>();
            s.lr.startWidth = lineWidth;
            s.lr.endWidth = lineWidth;
            s.lr.positionCount = 2;
            s.lr.material = lineMaterial;

            // BoxCollider
            s.col = lineObj.AddComponent<BoxCollider>();
            s.col.isTrigger = false;
            lineObj.layer = LayerMask.NameToLayer("hand"); // Layer da mão

            segments.Add(s);
        }
    }

    void Update()
    {
        // Atualiza cada segmento (posição + collider)
        foreach (var s in segments)
        {
            Vector3 p1 = s.a.position;
            Vector3 p2 = s.b.position;

            // Linha
            s.lr.SetPosition(0, p1);
            s.lr.SetPosition(1, p2);

            // Collider
            Vector3 center = (p1 + p2) / 2f;
            Vector3 dir = p2 - p1;
            float length = dir.magnitude;

            s.col.transform.position = center;
            if (dir != Vector3.zero)
                s.col.transform.rotation = Quaternion.LookRotation(dir);
            s.col.size = new Vector3(colliderThickness, colliderThickness, length);
        }

        // Pinch detection
        Transform thumbTip = points[4];
        Transform indexTip = points[8];

        bool pinching = Vector3.Distance(thumbTip.position, indexTip.position) < pinchThreshold;
        if (pinching && !wasPinching)
        {
            TryGrabObject(indexTip);
        }
        else if (!pinching && wasPinching)
        {
            ReleaseObject();
        }

        wasPinching = pinching;
    }

    void TryGrabObject(Transform pinchTip)
    {
        Collider[] hits = Physics.OverlapSphere(pinchTip.position, grabRadius, grabbableLayer);
        if (hits.Length > 0)
        {
            Rigidbody potentialGrab = hits[0].attachedRigidbody;

            if (potentialGrab == null)
            {
                // 2. Se for nulo, busque na hierarquia pai (o objeto Rigidbody pode ser o pai)
                potentialGrab = hits[0].GetComponentInParent<Rigidbody>();
            }

            grabbedObject = potentialGrab;
            if (grabbedObject != null)
            {
                // Anexa o objeto ao dedo
                print("success");
                grabbedObject.isKinematic = true;
                grabbedObject.transform.SetParent(pinchTip);
                grabbedObject.transform.localPosition = Vector3.zero;
                grabbedObject.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            // Solta o objeto
            grabbedObject.transform.SetParent(null);
            grabbedObject.isKinematic = false;
            grabbedObject = null;
        }
    }
}