using UnityEngine;
using Unity.Entities;

public class Castle : MonoBehaviour
{
    public int MaxCastleHP;
    public int CurrCastleHP;
    public GameObject Shield;
    public TextMesh HPText;

    private InGameOperation sceneManager;
    private StageManager stageManager;
    private AudioManager audioManager;
    private AudioSource audioSource;

    private CastleSpawner castleSpawner;

    // Start is called before the first frame update
    void Start()
    {
        sceneManager = FindObjectOfType<InGameOperation>();
        stageManager = FindObjectOfType<StageManager>();
        audioManager = FindObjectOfType<AudioManager>();
        castleSpawner = FindObjectOfType<CastleSpawner>();
        audioSource = GetComponent<AudioSource>();

        MaxCastleHP = (int)PlayerPrefs.GetFloat("hpMax");
        CurrCastleHP = MaxCastleHP;
    }

    // Update is called once per frame
    private void Update()
    {
        int PreviousCastleHP = CurrCastleHP;
        CurrCastleHP = GetCastleHpFromEntity();
        HPText.text = CurrCastleHP.ToString();
        if (PreviousCastleHP > CurrCastleHP)
            audioSource.PlayOneShot(audioManager.GetAudio("se_Hitted"));
        Shield.SetActive(MaxCastleHP > 1);
    }

    public bool AddedHealth(int Val = 1)
    {
        if (CurrCastleHP > 0)
            SetCastleHpToEntity(CurrCastleHP + 1);
        return CurrCastleHP <= 0;
    }

    public void SetCastleHpToEntity(int hp)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.SetComponentData(castleSpawner.Entities[0], new Health
        {
            Value = hp
        });
    }
    public int GetCastleHpFromEntity() {
        if (castleSpawner.castleHPArray.Length > 0)
            return castleSpawner.castleHPArray[0];
        return -1;
    }
}