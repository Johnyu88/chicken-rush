using System.Collections.Generic;
using UnityEngine;

namespace ChickenRush
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class ChickenAnger : MonoBehaviour
    {
        public bool isAngry { get; private set; }
        private Rigidbody2D body;
        private SpriteRenderer visual;
        private Collider2D[] colliders;
        private readonly List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        private PhysicsMaterial2D angryMaterial;
        private float squeezedTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            visual = GetComponent<SpriteRenderer>();
            colliders = GetComponents<Collider2D>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.relativeVelocity.magnitude > 6f) BecomeAngry();
        }

        private void FixedUpdate()
        {
            if (isAngry) return;
            if (!body.simulated) { squeezedTime = 0f; return; }
            // Query contacts even when sleeping: OnCollisionStay2D is not sent for sleeping bodies.
            contacts.Clear();
            body.GetContacts(contacts);
            bool squeezed = false;
            for (int i = 0; i < contacts.Count && !squeezed; i++)
                for (int j = i + 1; j < contacts.Count; j++)
                    if (Vector2.Dot(contacts[i].normal, contacts[j].normal) < -0.5f)
                    { squeezed = true; break; }
            squeezedTime = squeezed ? squeezedTime + Time.fixedDeltaTime : 0f;
            if (squeezedTime > 1.5f) BecomeAngry();
        }

        private void BecomeAngry()
        {
            if (isAngry || !body.simulated) return;
            isAngry = true;
            visual.color = Color.red;
            transform.localScale *= 1.2f;
            // Never mutate the shared prefab material: anger belongs to this chicken only.
            angryMaterial = new PhysicsMaterial2D("ChickenAngry (Instance)")
            { friction = 0.2f, bounciness = 0.8f };
            body.sharedMaterial = angryMaterial;
            foreach (var collider in colliders) collider.sharedMaterial = angryMaterial;
            body.WakeUp();
        }

        private void OnDisable() { squeezedTime = 0f; }
        private void OnDestroy()
        {
            if (angryMaterial != null) Destroy(angryMaterial);
        }
    }
}
