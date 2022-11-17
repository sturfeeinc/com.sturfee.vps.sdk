using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void DigitalTwinFeatureEvent(DigitalTwinFeature feature);


public class DigitalTwinFeature : MonoBehaviour
{
    public DigitalTwinFeatureEvent OnDigitalTwinFeatureChange;

    public string FeatureId;    
    public FeatureLayer FeatureLayer;
    public bool IsVisible = true;

    public List<Guid> DtEnhancementIds = new List<Guid>();

    
    public bool RemoveFeature
    {
        get { return _hideMe; }
        set {
            _hideMe = value;
            OnDigitalTwinFeatureChange?.Invoke(this);
        }
    }
    [Header("Internal")]
    [SerializeField]
    private bool _hideMe;
}
