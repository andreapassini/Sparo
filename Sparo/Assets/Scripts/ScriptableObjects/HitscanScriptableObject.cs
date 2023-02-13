   
using UnityEngine;


namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/HitscanScriptableObject", order = 1)]
    public class HitscanScriptableObject: ScriptableObject
    {
        public readonly bool stopWithCooldown;
        [Tooltip("used ony if stopWithAnimation is false")]
        public readonly float cooldown;
        
        [Space]
        public readonly float damage;

        [Space] 
        public readonly GameObject vfxShootEffect;

        [Space] 
        public readonly LayerMask hitMask;
    }
}