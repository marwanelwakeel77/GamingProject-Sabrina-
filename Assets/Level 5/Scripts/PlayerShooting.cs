using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//put this on alex (like ABShooting in lab 9). The Shoot key makes a bullet at the ShootingPoint.
//10 bullets, then a reload of 4 seconds
public class PlayerShooting : MonoBehaviour
{
    public KeyCode Shoot = KeyCode.F;
    public Transform shootingPoint; //empty child near alex's hand
    public GameObject bullet; //the bullet prefab
    public SpriteRenderer sr;

    public int maxAmmo = 10;
    public float reloadTime = 4f;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    public TextMeshProUGUI ammoText; //"AMMO 10 / 10"
    public Image[] bulletIcons; //10 small bullets under the text
    public GameObject reloadBox;
    public Image reloadBar;
    public TextMeshProUGUI reloadText;

    private int ammo;
    private bool reloading = false; //so only one reload runs at a time
    private float pointX;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        pointX = shootingPoint.localPosition.x;
        ammo = maxAmmo;
        reloadBox.SetActive(false);
        ShowAmmo();
    }

    // Update is called once per frame
    void Update()
    {
        //the shooting point is on the side alex is looking at
        if (sr.flipX == true)
        {
            shootingPoint.localPosition = new Vector3(-pointX, shootingPoint.localPosition.y, 0);
        }
        else
        {
            shootingPoint.localPosition = new Vector3(pointX, shootingPoint.localPosition.y, 0);
        }

        if (Input.GetKeyDown(Shoot) && ammo > 0 && reloading == false)
        {
            Shooting();
        }
    }

    public void Shooting()
    {
        Instantiate(bullet, shootingPoint.position, shootingPoint.rotation); //like lab 9
        Level5Manager.Instance.PlaySound(shootSound);
        ammo--;
        ShowAmmo();

        if (ammo == 0)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        reloading = true;
        reloadBox.SetActive(true);
        Level5Manager.Instance.PlaySound(reloadSound);

        float t = 0f;
        while (t < reloadTime)
        {
            t = t + Time.deltaTime;
            reloadBar.fillAmount = t / reloadTime;
            reloadText.text = "RELOADING " + (int)(reloadTime - t + 1) + "s";
            yield return null;
        }

        ammo = maxAmmo;
        reloading = false;
        reloadBox.SetActive(false);
        ShowAmmo();
    }

    void ShowAmmo()
    {
        ammoText.text = "AMMO  " + ammo + " / " + maxAmmo;
        for (int i = 0; i < bulletIcons.Length; i++)
        {
            bulletIcons[i].enabled = i < ammo; //the used bullets disappear
        }
    }
}
