using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float m_DampTime = 0.2f;                 // tiempo de enfocar otra vez.
    public float m_ScreenEdgeBuffer = 4f;           // espacio entre arriba y abajo.
    public float m_MinSize = 6.5f;                  // minimo tamaño de la camara.
    [HideInInspector] public Transform[] m_Targets; // array con todas las posiciones que necesita la camara.


    private Camera m_Camera;                       
    private float m_ZoomSpeed;                      // velocidad de smooth.
    private Vector3 m_MoveVelocity;                 // velocidad de referencia para la posicion con smooth.
    private Vector3 m_DesiredPosition;              // la direccion de la camara.


    private void Awake ()
    {
        m_Camera = GetComponentInChildren<Camera> ();
    }


    private void FixedUpdate ()
    {
        // mover la camara.
        Move ();

        // cambiar el zoom para abrir la camara.
        Zoom ();
    }


    private void Move ()
    {
        // encontrar la posicion de los objetivos.
        FindAveragePosition ();

        // smooth a posicion.
        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }


    private void FindAveragePosition ()
    {
        Vector3 averagePos = new Vector3 ();
        int numTargets = 0;

        // comprobar todos los objetivos y añadirlos.
        for (int i = 0; i < m_Targets.Length; i++)
        {
            // si no esta activo pasa al siguiente
            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            // incrementos de los objetivos.
            averagePos += m_Targets[i].position;
            numTargets++;
        }

        // calcular la suma de las posiciones si hay objetivos.
        if (numTargets > 0)
            averagePos /= numTargets;

        // mantener valos de y
        averagePos.y = transform.position.y;

        // igualamos la posicion que queremos a la posicion media;
        m_DesiredPosition = averagePos;
    }


    private void Zoom ()
    {
        // ajustar el tamaño y hacer smooth a ese tamaño
        float requiredSize = FindRequiredSize();
        m_Camera.orthographicSize = Mathf.SmoothDamp (m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }


    private float FindRequiredSize ()
    {
        // posicion de la camara hacia la que se mueve
        Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

        // tamaño camara a 0
        float size = 0f;

        // recorremos los objetivos
        for (int i = 0; i < m_Targets.Length; i++)
        {
            // ...si esta inactivo pasa al siguiente
            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            // posicion del ojetivo en la camara
            Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

            // encontrams la posicion del objetivo desde donde queremos la posicion de la camara
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            // elegimos el mayor tamaño posible y la distancia de arriba y abajo
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

            // lo mismo pero derecha e izquierda
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
        }

        // buffer para el tamaño
        size += m_ScreenEdgeBuffer;

        // comprueba que el tamño de la camara no se pase del minimo
        size = Mathf.Max (size, m_MinSize);

        return size;
    }


    public void SetStartPositionAndSize ()
    {
        // posicion deseada
        FindAveragePosition ();

        // camara a posicion deseada sin smooth
        transform.position = m_DesiredPosition;

        // configurar el tamaño adecudao de la camara
        m_Camera.orthographicSize = FindRequiredSize ();
    }
}
    
