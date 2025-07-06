using UnityEngine;
using UnityEngine.Serialization;

namespace GameLogic
{
    public class ComponentView: MonoBehaviour
    {
        [FormerlySerializedAs("Type")] public string type;
        public object Component { get; set; }
    }
}