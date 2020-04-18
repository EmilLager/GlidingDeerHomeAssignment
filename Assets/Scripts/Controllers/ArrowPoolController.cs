using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArrowPoolController : MonoBehaviour
{

    #region SerializedFields

    [SerializeField] private Transform m_arrowPrefab; //I usually use a controller component reference but the arrow prefab doesn't need one

    [SerializeField] private int m_initialArrowPoolSize;
    
    [SerializeField] private List<Transform> m_arrowList; //Not used here aside from validation but useful for general purposes when using object pooling, is serialized for debugging purposes

    [SerializeField] private ArrowFlightSettings arrowFlightSettings;
    
    #endregion

    #region PrivateFields
    
    private List<Transform> m_availableArrowList;
    
    private Queue<Transform> m_arrowsInTarget;
    
    private Transform AvailableArrow {
        get {
            Transform arrow = null;
            if (m_availableArrowList.Count > 0) {
                arrow = m_availableArrowList[0];
                m_availableArrowList.Remove(arrow);
                
            } else {
                arrow = m_arrowsInTarget.Dequeue();
                arrow.transform.SetParent(null);
            }

            arrow.gameObject.SetActive(true);
            return arrow;
        }
    }

    #endregion

    #region PublicFunctions

    public void ShootArrow(Vector3 hitPosition, bool hitTarget, Action<Transform> onHit) {
        StartCoroutine(ArrowFlightRoutine(AvailableArrow, hitPosition, hitTarget, onHit));
    }

    public void ReturnArrowToPool(Transform arrow) {
        if (m_arrowList.Contains(arrow)) {
            arrow.gameObject.SetActive(false);
            arrow.transform.SetParent(null);
            m_availableArrowList.Add(arrow);
        }
    }

    #endregion
    
    #region UnityCallbacks
    private void Awake() {
        m_arrowsInTarget = new Queue<Transform>();
        m_availableArrowList = new List<Transform>();
        for (int i = 0; i < m_initialArrowPoolSize; i++) {
            Transform newArrow = Instantiate(m_arrowPrefab);
            m_arrowList.Add(newArrow);
            m_availableArrowList.Add(newArrow);
            newArrow.gameObject.SetActive(false);
        }
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (arrowFlightSettings == null) {
            arrowFlightSettings = ArrowFlightSettings.Load();
        }
    }
#endif
    
    #endregion
    
    #region PrivateMethods
    
    private IEnumerator ArrowFlightRoutine(Transform arrow, Vector3 finalTarget, bool hitTarget, Action<Transform> onHit) {

        bool targetReached = false;
        float pathTraversed = 0f;
        Vector3 initialPosition = finalTarget + arrowFlightSettings.ArrowStartOffset * Vector3.back;
        float angleOffset = Random.Range(-arrowFlightSettings.MaximalTrajectoryDeviationAngle, arrowFlightSettings.MaximalTrajectoryDeviationAngle);
        
        arrow.position = initialPosition;
        
        while (!targetReached) {
            pathTraversed += Time.deltaTime * arrowFlightSettings.ArrowSpeed;

            if (pathTraversed >= 1f) { //We are at the target position
                if (hitTarget) { //Stop arrow at target
                    targetReached = true;
                    arrow.position = finalTarget;
                    m_arrowsInTarget.Enqueue(arrow);
                } else { //Let arrow continue flight according to settings
                    arrow.position += Time.deltaTime * arrowFlightSettings.ArrowSpeed * (initialPosition - finalTarget).magnitude * arrow.forward;
                    if (pathTraversed >= (arrowFlightSettings.MissExtendedFlightLength / arrowFlightSettings.ArrowStartOffset) + 1) {
                        targetReached = true;
                    }
                }
            } else { //Arrow in initial flight
                Vector3 arrowPosition = Vector3.Lerp(initialPosition, finalTarget, pathTraversed);

                arrowPosition += Quaternion.AngleAxis(angleOffset, Vector3.forward) * Vector3.up * arrowFlightSettings.ArrowFlightCurve.Evaluate(pathTraversed);

                arrow.forward = arrowPosition - arrow.position; //Make arrow face flight direction

                arrow.position = arrowPosition;
            }

            yield return null;
        }
        onHit?.Invoke(arrow);
    }
    
    #endregion
}