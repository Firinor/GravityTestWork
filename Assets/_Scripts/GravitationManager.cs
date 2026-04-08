using System;
using UnityEngine;

public class GravitationManager : MonoBehaviour
{
    private GravityObject[] gravityObjects;
    private Character player;

    [SerializeField] private float minForce;
    [SerializeField] private float maxForce;

    private void Awake()
    {
        gravityObjects 
            = FindObjectsByType<GravityObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        player = FindFirstObjectByType<Character>();
    }

    private void Update()
    {
        GravityObject closestObject = null;
        float distance = float.MaxValue;
        foreach (var gravityObject in gravityObjects)
        {
            if(!gravityObject.isActiveAndEnabled) continue;
            
            float newObjectDistance = Vector2.Distance(player.transform.position, gravityObject.transform.position);

            if (newObjectDistance < distance)
            {
                distance = newObjectDistance;
                closestObject = gravityObject;
            }
        }
        
        float gravityForce = closestObject.Mass / (distance * distance);
        gravityForce = Mathf.Max(gravityForce, minForce);
        gravityForce = Mathf.Min(gravityForce, maxForce);
            
        Vector2 forceDirection = (closestObject.transform.position - player.transform.position).normalized;
        player.GravityObject = closestObject;
        player.Gravity = forceDirection * gravityForce;
    }
}
