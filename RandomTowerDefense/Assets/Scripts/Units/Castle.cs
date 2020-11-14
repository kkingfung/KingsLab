using UnityEngine;
using Unity.Entities;

public class Castle : MonoBehaviour
{
    public int MaxCastleHP;
    public int CurrCastleHP;
    public GameObject Shield;
    public TextMesh HPText;

    private AudioManager audioManager;
    private AudioSource audioSource;
    private StageManager stageManager;
    private CastleSpawner castleSpawner;
    private TutorialManager tutorialManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        castleSpawner = FindObjectOfType<CastleSpawner>();
        audioSource = GetComponent<AudioSource>();
        stageManager = FindObjectOfType<StageManager>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        MaxCastleHP = (int)PlayerPrefs.GetFloat("hpMax",5);
        CurrCastleHP = MaxCastleHP;
    }

    // Update is called once per frame
    private void Update()
    {
        int PreviousCastleHP = CurrCastleHP;
        CurrCastleHP = GetCastleHpFromEntity();

        HPText.text = CurrCastleHP < 0 ? "0" : CurrCastleHP.ToString();
        if (PreviousCastleHP > CurrCastleHP) {
            AddedHealth(0);
            if(audioManager.enabledSE)
                audioSource.PlayOneShot(audioManager.GetAudio("se_Hitted"));
        }
           

        Shield.SetActive(CurrCastleHP > 1);
    }

    public bool AddedHealth(int Val = 1)
    {
        castleSpawner.castleHPArray[0] = CurrCastleHP + Val;
        if (CurrCastleHP + Val <= 0)
        {
            if (stageManager.CheckLose())
                tutorialManager.DestroyAllRelated();
        }
        if (CurrCastleHP > 0)
            SetCastleHpToEntity(CurrCastleHP + Val);
                 
        return CurrCastleHP <= 0;
    }

    public void SetCastleHpToEntity(int hp)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.SetComponentData(castleSpawner.Entities[0], new Health
        {
            Value = hp
        });

        if (MaxCastleHP < hp)
            MaxCastleHP = hp;
    }
    public int GetCastleHpFromEntity() {
        if (castleSpawner.castleHPArray.Length > 0)
            return castleSpawner.castleHPArray[0];
        return -1;
    }
}