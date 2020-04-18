using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TargetController : MonoBehaviour {

    #region SerializedFields

    [Header("References")]
    [SerializeField] private Animator m_animator;
    
    [SerializeField] private AudioSource m_audioSource;
    
    [SerializeField] private AudioClip m_missSound;
    
    [SerializeField] private AudioClip m_greenHitSound;
    
    [SerializeField] private AudioClip m_yellowHitSound;
    
    [SerializeField] private AudioClip m_redHitSound;
    
    [SerializeField] private Transform m_targetCenter;

    [SerializeField] private ArrowPoolController m_arrowPool;
    
    [SerializeField] private ParticleSystem m_hitEffect;

    [SerializeField] private ParticleSystem m_bullsEyeEffect;

    [Header("Animation settings")]
    
    [SerializeField] private string m_hitTrigger;
    
    [SerializeField] private string m_bullsEyeTrigger;
    
    [SerializeField] private string m_animationVariationFloat;

    [Header("Hit zone settings")]
    [SerializeField] private List<TargetAreaZone> m_targetAreaZones;
    
    #endregion

    #region PrivateFields
    
    private Dictionary<TargetAreas, TargetAreaZone> m_targetAreaZonesDict;
    
    private int m_hitTriggerHash;
    
    private int m_bullsEyeTriggerHash;
    
    private int m_animationVariationFloatHash;
    
    #endregion
    
    #region PublicFunctions

    public void ShootArrow(TargetAreas targetArea, Action onComplete) {

        m_arrowPool.ShootArrow(GetHitPosition(targetArea), targetArea != TargetAreas.Miss,
            arrow => { OnArrowFlightComplete(targetArea, onComplete, arrow); });
    }

    #endregion
    
    #region PrivateFunctions
    
    private Vector3 GetHitPosition(TargetAreas targetArea) {

        Vector3 hitPosition = 
            Quaternion.AngleAxis(Random.Range(0f, 360f), m_targetCenter.forward)
            * m_targetCenter.up;
        
        hitPosition *= Random.Range(m_targetAreaZonesDict[targetArea].MinimalRadius, m_targetAreaZonesDict[targetArea].MaximalRadius);

        return m_targetCenter.position + hitPosition;
    }

    private void OnArrowFlightComplete(TargetAreas targetArea, Action onComplete, Transform arrow) {
        
        m_hitEffect.gameObject.transform.position = arrow.transform.position;
        m_hitEffect.Stop();
        m_hitEffect.Play();

        switch (targetArea) {
            case TargetAreas.Red:
                m_bullsEyeEffect.Stop();
                m_bullsEyeEffect.Play();
                m_animator.SetFloat(m_animationVariationFloatHash, Random.value);
                m_animator.SetTrigger(m_bullsEyeTriggerHash);
                arrow.SetParent(m_animator.transform);
                m_audioSource.PlayOneShot(m_redHitSound);
                break;
            case TargetAreas.Miss:
                m_arrowPool.ReturnArrowToPool(arrow);
                m_audioSource.PlayOneShot(m_missSound);
                break;
            default:
                m_animator.SetFloat(m_animationVariationFloatHash, Random.value);
                m_animator.SetTrigger(m_hitTriggerHash);
                m_audioSource.PlayOneShot(targetArea == TargetAreas.Green ? m_greenHitSound : m_yellowHitSound);
                arrow.SetParent(m_animator.transform);
                break;
        }

        StartCoroutine(ReportComplete(onComplete));
    }

    private IEnumerator ReportComplete(Action onComplete) {
        yield return null; //Wait one frame so the animation starts
        yield return new WaitForSeconds(m_animator.GetCurrentAnimatorStateInfo(0).length);
        onComplete?.Invoke();
    }

    #endregion
    
    #region UnityCallbacks
    
    private void Awake() {
        m_targetAreaZonesDict = new Dictionary<TargetAreas, TargetAreaZone>();
        
        foreach (TargetAreaZone targetAreaZone in m_targetAreaZones) {
            m_targetAreaZonesDict.Add(targetAreaZone.Area, targetAreaZone);
        }
        
        m_hitTriggerHash = Animator.StringToHash(m_hitTrigger);
        m_bullsEyeTriggerHash = Animator.StringToHash(m_bullsEyeTrigger);
        m_animationVariationFloatHash = Animator.StringToHash(m_animationVariationFloat);
    }
    
    
#if UNITY_EDITOR
    private void OnValidate() {

        //Validate animator
        if (!m_animator) { m_animator = GetComponent<Animator>(); }
        
        //Validate audioSource
        if (!m_audioSource) { m_audioSource = GetComponent<AudioSource>(); }
        
        //Validate arrow pool
        if (!m_arrowPool) { m_arrowPool = GetComponent<ArrowPoolController>(); }
        
        //Validate animator parameters
        if (!m_animator.parameters.Any(parameter => parameter.name.Equals(m_hitTrigger))) { m_hitTrigger = null; }
        
        if (!m_animator.parameters.Any(parameter => parameter.name.Equals(m_bullsEyeTrigger))) { m_bullsEyeTrigger = null; }
        
        if (!m_animator.parameters.Any(parameter => parameter.name.Equals(m_animationVariationFloat))) { m_animationVariationFloat = null; }

        //Make sure all values are present and only once
        foreach (TargetAreas area in Enum.GetValues(typeof(TargetAreas))) {
            
            int areaCount = m_targetAreaZones.Count(zone => zone.Area == area);
            
            if (areaCount == 0) { //Add if missing
                m_targetAreaZones.Add(new TargetAreaZone(area));
            } else if(areaCount > 1){ //Remove all but 1 if too many
                for (int i = 0; i < areaCount - 1; i++) {
                    m_targetAreaZones.Remove(m_targetAreaZones.FindLast(zone => zone.Area == area));
                }
            }
        }
        
        //Sort
        m_targetAreaZones = m_targetAreaZones.OrderBy(area => area.Area).ToList();

        //Validate radius
        m_targetAreaZones[0].SetMinimalRadius(0f);
        
        for (int i = 1; i < m_targetAreaZones.Count; i++) {
            m_targetAreaZones[i].SetMinimalRadius(m_targetAreaZones[i-1].MaximalRadius);
        }
    }

    private void OnDrawGizmos() {
        foreach (TargetAreaZone targetAreaZone in m_targetAreaZones) {
            Gizmos.color = targetAreaZone.PreviewColor;
            Gizmos.DrawSphere(m_targetCenter.position, targetAreaZone.MaximalRadius);
        }
    }
#endif
    
    #endregion

    #region Testing
    
#if UNITY_EDITOR
    [ContextMenu("ShootRed")]
    public void ShootAtRed() {
        ShootArrow(TargetAreas.Red, null);
    }

    [ContextMenu("ShootYellow")]
    public void ShootYellow() {
        ShootArrow(TargetAreas.Yellow, null);
    }
    [ContextMenu("ShootGreen")]
    public void ShootGreen() {
        ShootArrow(TargetAreas.Green, null);
    }
    [ContextMenu("ShootMiss")]
    public void ShootMiss() {
        ShootArrow(TargetAreas.Miss, null);
    }
#endif

    #endregion
    
    [System.Serializable]
    private class TargetAreaZone {
        [SerializeField] private TargetAreas m_area;
        [SerializeField] private float m_minimalRadius;
        [SerializeField] private float m_maximalRadius;
        [SerializeField] private Color m_previewColor;

        public TargetAreas Area => m_area;
        public float MinimalRadius => m_minimalRadius;
        public float MaximalRadius => m_maximalRadius;

        public Color PreviewColor => m_previewColor;
        
        public TargetAreaZone(TargetAreas area) {
            m_area = area;
        }

        public void SetMinimalRadius(float minimalRadius) {
            m_minimalRadius = minimalRadius;
            m_maximalRadius = Mathf.Max(minimalRadius, m_maximalRadius);
        }
    }
}

public enum TargetAreas {
    Red,
    Yellow,
    Green,
    Miss,
}