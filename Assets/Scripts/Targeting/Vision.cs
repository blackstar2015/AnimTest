using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    private float _range = 5f;
    private float _FOV = 120f;             // field of view
    private LayerMask _visibilityMask;     // layer(s) for visible objects
    private LayerMask _occlusionMask;      // layer(s) that block vision (usually walls)
    private int CurrentVisibleIndex;
    public Vector3 LookPosition => transform.position + Vector3.up;
    public Vector3 LookDirection => transform.forward;

    private void Awake()
    {
        if(gameObject.TryGetComponent(out StateMachine stateMachine))
        {
            _range = stateMachine.Range;
            _FOV = stateMachine.FOV;
            _visibilityMask = stateMachine.VisibilityMask;
            _occlusionMask = stateMachine.OcclusionMask;
            CurrentVisibleIndex = stateMachine.CurrentVisibleIndex;
        }
    }


    public bool TestVisibility(Vector3 point)
    {
        // we're using early returns and stopping if any test fails
        // we start with the cheaper tests first
        
        // distance
        float distance = Vector3.Distance(LookPosition, point);
        if(distance > _range) return false;
        
        // angle
        Vector3 dirToPoint = (point - LookPosition).normalized;
        float angle = Vector3.Angle(LookDirection, dirToPoint);
        float halfFOV = _FOV * 0.5f;
        if(angle > halfFOV) return false;
        
        // occlusion
        // LineCast checks collision between two points, we don't need a distance
        if (Physics.Linecast(LookPosition, point, _occlusionMask)) return false;
        
        // passed all tests, point is visibile
        return true;
    }

    public List<Targetable> GetVisibleTargets(int team)
    {
        List<Targetable> targets = new List<Targetable>();
    
        // find nearby colliders in range
        Collider[] hits = Physics.OverlapSphere(LookPosition, _range, _visibilityMask);
    
        // filter and add valid targets
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;                         // skip ourselves
            if (!hit.TryGetComponent(out Targetable targetable)) continue;      // skip objects without Targetable
            if (targetable.Team == team) continue;                              // skip same team
            if (!targetable.IsTargetable) continue;                             // skip not targetable
            if (!TestVisibility(targetable.ViewPosition.position)) continue;    // skip not visible
    
            // all tests passed, add target to list
            targets.Add(targetable);
        }    
        return targets;
    }
    
    public Targetable GetFirstVisibleTarget(int team)
    {
        List<Targetable> targets = GetVisibleTargets(team);
        if (targets.Count == 0) return null;
        return targets[CurrentVisibleIndex]; 
    
        // more sophisticated AI could score and rank targets to pick the best approach
        // ex: distance to, flanking, health %, damage vulnerabilities, threat
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LookPosition, _range);
        // slick one-liners look cool but your team will hate you
        Gizmos.DrawRay(LookPosition, transform.rotation * Quaternion.Euler(0f, _FOV * 0.5f, 0f) * Vector3.forward * _range);
        Gizmos.DrawRay(LookPosition, transform.rotation * Quaternion.Euler(0f, -_FOV * 0.5f, 0f) * Vector3.forward * _range);
    }
}
