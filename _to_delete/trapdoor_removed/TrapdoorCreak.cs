using UnityEngine;

// The trapdoor itself is the Lab 5 hinge joint (Exercise #16) and needs no code.
// This small script only plays a creak when the trapdoor swings open.
public class TrapdoorCreak : MonoBehaviour
{
    public AudioClip creakSound;
    public float openAngle = 20f; // more than this many degrees = the trapdoor is open

    private HingeJoint2D hinge;
    private bool isOpen = false;

    void Start()
    {
        hinge = GetComponent<HingeJoint2D>();
    }

    void Update()
    {
        bool openNow = Mathf.Abs(hinge.jointAngle) > openAngle;

        if (openNow == true && isOpen == false && AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(creakSound);

        isOpen = openNow;
    }
}
