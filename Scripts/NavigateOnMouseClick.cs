using UnityEngine;
using UnityEngine.AI;

public class NavigateOnMouseClick : MonoBehaviour
{

    public enum MouseButtonType { Left, Right, Middle };
    public MouseButtonType mouseButton = MouseButtonType.Left;
    public string speedParameter = "Speed";
    public float distanceThreshold = 0.5f;

    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Animator animator;


    void Update()
    {
        var speed = (navMeshAgent.remainingDistance < distanceThreshold) ? 0 : 1;
        if (animator != null) animator.SetFloat("Speed", speed);

        // Moves the Player if the Mouse Button was clicked:
        if (Input.GetMouseButtonDown((int)mouseButton) && GUIUtility.hotControl == 0)
        {
            RaycastHit _hit;
            Ray pointRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(pointRay, out _hit, 100.0f);


            navMeshAgent.SetDestination(_hit.point);

        }

        // Moves the player if the mouse button is held down:
        //else if (Input.GetMouseButton((int)mouseButton) && GUIUtility.hotControl == 0)
        //{
        //    Plane playerPlane = new Plane(Vector3.up, transform.position);
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    float hitdist = 0.0f;
        //    if (playerPlane.Raycast(ray, out hitdist))
        //    {
        //        navMeshAgent.SetDestination(ray.GetPoint(hitdist));
        //    }
        //}
    }
}