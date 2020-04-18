using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarController : MonoBehaviour {
    
    #region Serialized fields
    
    [Header("References")]
    [SerializeField] private Transform m_barIndicator;
    
    [SerializeField] private Transform m_leftEdge;
    
    [SerializeField] private Transform m_rightEdge;
        
    #endregion
    
    #region PublicMethods

    public void SetIndicatorProgress(float progress) {
        m_barIndicator.transform.position = Vector3.Lerp(m_leftEdge.position, m_rightEdge.position, progress);
    }

    #endregion
}
