using System;
using UnityEngine;

public class MouseEventService : MonoBehaviour
{
    public static MouseEventService Instance { get; private set; }

    public Action<Vector3> mapClicked;
    public Action<Transform> enemyClicked;
    public Action<Cell> cellClicked;

    public Action nextStep;
    public Action prevStep;



    //[SerializeField] private KamikazeObject _debugPoint;

    private const float _MAX_RAYCAST_DISTANCE = 20f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            ThrowRaycast();
        if (Input.GetKeyDown(KeyCode.RightArrow))
            nextStep();
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            prevStep();
    }

    private void ThrowRaycast()
    {
        RaycastHit _hit;
        Ray pointRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(pointRay, out _hit, _MAX_RAYCAST_DISTANCE))
        {
            if (_hit.collider == null) return;
            //Instantiate(_debugPoint, _hit.point, Quaternion.identity);
            //Debug.Log(_hit.collider.name);
            if (_hit.collider.tag == "Cell")
                cellClicked?.Invoke(_hit.collider.GetComponent<Cell>());

            //if (_hit.collider.tag == "Enemy")
            //{
            //    enemyClicked?.Invoke(_hit.collider.transform);
            //    Debug.Log("Clicked to enemy");
            //}
            //else
            //{
            //    mapClicked?.Invoke(_hit.point);
            //    Debug.Log("Clicked to ground");
            //}
        }
    }
}
