using UnityEngine;

/// <summary>
/// Renders the connection between 2 nodes in the graph display.
/// </summary>
public class LineBetweenNodes : MonoBehaviour
{
    public Transform nodeA;
    public Transform nodeB;
    public float lineWidth = 0.1f;
    public Material lineMaterial;

    private GameObject lineObject;

    /// <summary>
    /// Initialize the line between nodes.
    /// </summary>
    /// <param name="nodeA">the start node</param>
    /// <param name="nodeB">the target node</param>
    /// <param name="lineWidth">line width</param>
    /// <param name="lineMaterial">the material for the line object</param>
    /// <param name="lineName">the name of the line that this is rendering</param>
    public void Initialize(Transform nodeA, Transform nodeB, float lineWidth, Material lineMaterial, string lineName)
    {
        if (nodeA == null || nodeB == null) return;

        this.nodeA = nodeA;
        this.nodeB = nodeB;
        this.lineWidth = lineWidth;
        this.lineMaterial = lineMaterial;

        // create the renderer object
        lineObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lineObject.transform.parent = transform;
        lineObject.isStatic = true;
        Destroy(lineObject.GetComponent<Collider>());
        lineObject.GetComponent<MeshRenderer>().material = lineMaterial;

        if (lineObject.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = SkytrainLoader.SkytrainLineColors[lineName];
        }

        UpdateLine();
    }

    void Update()
    {
        if (nodeA != null && nodeB != null)
        {
            UpdateLine();
        }
    }

    /// <summary>
    /// Update the current line position.
    /// </summary>
    void UpdateLine()
    {
        Vector3 start = nodeA.position;
        Vector3 end = nodeB.position;
        Vector3 center = (start + end) / 2;
        Vector3 direction = end - start;
        float length = direction.magnitude;

        lineObject.transform.position = center;
        lineObject.transform.rotation = Quaternion.LookRotation(direction);
        lineObject.transform.localScale = new Vector3(lineWidth, lineWidth, length);
    }
}

