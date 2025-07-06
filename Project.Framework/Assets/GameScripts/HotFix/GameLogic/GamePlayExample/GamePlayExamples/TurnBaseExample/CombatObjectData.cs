using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    
public class CombatObjectData : MonoBehaviour
{
    public AnimationComponent AnimationComponent;
    public Text DamageText;
    public Text CureText;
    public Image HealthBarImage;
    public Transform CanvasTrm;
    public Transform StatusSlotsTrm;
    public GameObject StatusIconPrefab;
    public GameObject vertigoParticle;
    public GameObject weakParticle;
}
}
