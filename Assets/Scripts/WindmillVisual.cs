using UnityEngine;

namespace ChickenRush
{
    public sealed class WindmillVisual : MonoBehaviour
    {
        public float degreesPerSecond = 55;
        // This component belongs to the rotor child, never to the collider object.
        private void Update() { transform.Rotate(0, 0, degreesPerSecond * Time.deltaTime); }
    }
}
