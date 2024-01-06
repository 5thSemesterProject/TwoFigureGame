using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAmbient : MonoBehaviour
{
    [SerializeField] private EAmbientSounds sound;
    [SerializeField] private float volume = 1;
    [SerializeField] private bool loop = true;
    [SerializeField] private float maxRange = 5;
    [Range(0.5f, 1.5f)][SerializeField] private float minPitch = 0.8f;
    [Range(0.5f, 1.5f)][SerializeField] private float maxPitch = 1.2f;

    #region  Gizmo Handling
    [Header("Gizmos")]
    [SerializeField] bool showAllGizmos = true;
    static bool _showAllGizmosGizmos;
    static List<SoundAmbient> allAmbients = new List<SoundAmbient>();
    //Filters
    [SerializeField]bool useFilter;
    static bool _useFilter;
    [SerializeField]List<EAmbientSounds> filterAmbientSounds = new List<EAmbientSounds>();
   
    static List<EAmbientSounds> _filterAmbientSounds = new List<EAmbientSounds>();

    private void  OnValidate()
    {           
        //Add All Ambient To one list
        if (!allAmbients.Contains(this))
            allAmbients.Add(this);

        //Sync Show All Gizmos
        for (int i = 0; i < allAmbients.Count; i++)
        {
            _showAllGizmosGizmos = showAllGizmos;

            if (allAmbients[i]!=this)
                allAmbients[i].SetShowAllGizmos(_showAllGizmosGizmos);      
        }

        //Sync Use Filter
        for (int i = 0; i < allAmbients.Count; i++)
        {
            _useFilter = useFilter;

            if (allAmbients[i]!=this)
                allAmbients[i].SetUseFilter(_useFilter);      
        }

        //Sync Filter
        for (int i = 0; i < allAmbients.Count; i++)
        {
            _filterAmbientSounds = filterAmbientSounds;

            if (allAmbients[i]!=this)
                allAmbients[i].SetFilterAmbientSounds(_filterAmbientSounds);      
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (_showAllGizmosGizmos &CheckFilter())
            Gizmos.DrawWireSphere(transform.position, maxRange);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (_showAllGizmosGizmos)
            Gizmos.DrawWireSphere(transform.position, maxRange);
    }

    public void SetShowAllGizmos(bool value)
    {
        showAllGizmos = value;
    }

    public void SetUseFilter(bool value)
    {
        useFilter = value;
    }

    public void SetFilterAmbientSounds(List<EAmbientSounds> input)
    {
        filterAmbientSounds = input;
    }

    bool CheckFilter()
    {
        if (_filterAmbientSounds.Contains(sound))
            return true;
        return !useFilter;
    }
    #endregion

    private IEnumerator Start()
    {
        while (true)
        {
            if (SoundSystem.Play(sound, this.transform, SoundPriority.None, loop, volume, 0, FadeMode.Default, 0, maxRange))
                yield break;

            yield return null;
        }
    }
}
