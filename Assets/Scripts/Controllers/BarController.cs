using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarController : MonoBehaviour {
    
    #region SerializedFields
    
    [Header("References")]
    [SerializeField] private Transform m_barIndicator;
    
    [SerializeField] private Transform m_leftEdge;
    
    [SerializeField] private Transform m_rightEdge;
        
    #endregion
    
    #region PrivateFields
    
    private Material m_indicatorMaterial;
    private readonly int m_colorPropertyIndex = Shader.PropertyToID("_Color");
    
    #endregion
    
    #region PublicMethods

    public void SetIndicatorProgress(float progress, Color indicatorColor) {
        m_barIndicator.transform.position = Vector3.Lerp(m_leftEdge.position, m_rightEdge.position, progress);
        m_indicatorMaterial.SetColor(m_colorPropertyIndex, indicatorColor);
    }

    #endregion
    
    #region UnityCallbacks

    private void Awake() {
        m_indicatorMaterial = m_barIndicator.GetComponent<Renderer>().material;
    }

    #endregion
}
