using UnityEngine;

public class Castle : MonoBehaviour
{
    public int MaxCastleHP;
    public int CurrCastleHP;
    public GameObject Shield;
    private GameObject obj;
    private InGameOperation sceneManager;
    private StageManager stageManager;
    private AudioManager audioManager;

    private AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        sceneManager = FindObjectOfType<InGameOperation>();
        stageManager = FindObjectOfType<StageManager>();
        audioManager = FindObjectOfType<AudioManager>();
        audio = GetComponent<AudioSource>();

            MaxCastleHP = (int)PlayerPrefs.GetFloat("hpMax");
        CurrCastleHP = MaxCastleHP;
    }

    // Update is called once per frame
    private void Update()
    {
            Shield.SetActive(CurrCastleHP > 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.GetMask("Enemy"))
        {
            stageManager.Damaged();
            other.gameObject.GetComponent<EnemyAI>().Die();
        }
    }

    public bool Damaged(int Val = 1)
    {
        CurrCastleHP -= Val;
        if(Val>0)  
        {
            audio.clip = audioManager.GetAudio("se_Hitted");
            audio.Play();
        }
            return CurrCastleHP <= 0;
    }

    public void SetObj(GameObject obj) {
        this.obj = obj;
    }
}