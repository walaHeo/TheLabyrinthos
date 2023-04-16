using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TypeDefs;

//같은 이름의 클래스가 두개라서 고정
using Slider = UnityEngine.UI.Slider;
using Image = UnityEngine.UI.Image;

public class UIModElements
{
    public UIModElements(Image hp, Image tp, Transform hit)
    {
        hpSlider = hp;
        tpSlider = tp;
        hitImage = hit;
    }
        
    public Image hpSlider;
    public Image tpSlider;
    public Transform hitImage;
}

public class BattleMain : MonoBehaviour
{
    BattleActions BA_battleActions;

    [SerializeField] bool b_playerReady;
    [SerializeField] bool b_enemyReady;

    [Header("Set in Inspector")]
    [SerializeField] public GameObject GO_hitImage;
    [SerializeField] TMP_Text TMP_playerDamage;
    [SerializeField] TMP_Text TMP_EnemyDamage;
    public GameObject GO_actionList;
    public GameObject GO_attackList;
    public Sprite SPR_playerAttack;
    public Sprite SPR_enemyAttack;

    public Slider SL_playerHP;
    public Slider SL_playerTP;
    public Slider SL_enemyHP;
    public Slider SL_enemyTP;
    public Transform TF_playerHitAnchor;
    public Transform TF_enemyHitAnchor;

    [Header("Set Automatically : SFX")]
    public AudioClip[] AC_playerAttackWeakPoint;
    public AudioClip[] AC_playerAttackThorax;
    public AudioClip[] AC_playerAttackOuter;
    public AudioClip[] AC_playerMissed;
    public AudioClip[] AC_enemyAttack;

    [HideInInspector] public Image IMG_playerHP;
    [HideInInspector] public Image IMG_playerTP;
    [HideInInspector] public Image IMG_enemyHP;
    [HideInInspector] public Image IMG_enemyTP;

    [HideInInspector] public UIModElements playerElements;
    [HideInInspector] public UIModElements enemyElements;

    [Header("Set in Inspector : Colors")]
    [SerializeField] public Color[] colors = new Color[5];

    public Dictionary<Parts, DmgAccText> dict_dmgAccList = new Dictionary<Parts, DmgAccText>();

    protected float f_enemySpeed = 0.0f;
    protected float f_playerSpeed = 0.0f;


    void Awake()
    {
        AC_playerAttackWeakPoint = Resources.LoadAll<AudioClip>("SFX/PlayerAttack/Weakpoint");
        AC_playerAttackThorax = Resources.LoadAll<AudioClip>("SFX/PlayerAttack/Thorax");
        AC_playerAttackOuter = Resources.LoadAll<AudioClip>("SFX/PlayerAttack/Outer");
        AC_playerMissed = Resources.LoadAll<AudioClip>("SFX/PlayerAttack/Miss");
        AC_enemyAttack = Resources.LoadAll<AudioClip>("SFX/EnemyAttack");
    }

    void Start()
    {
        BA_battleActions = GetComponent<BattleActions>();
        SL_playerTP.value = 0;
        StartBattleScene(GameManager.Instance.creatures.C_default[0]); //임시

        IMG_playerHP = SL_playerHP.transform.Find("Fill Area").GetChild(0).GetComponent<Image>();
        IMG_playerTP = SL_playerTP.transform.Find("Fill Area").GetChild(0).GetComponent<Image>();
        IMG_enemyHP = SL_enemyHP.transform.Find("Fill Area").GetChild(0).GetComponent<Image>();
        IMG_enemyTP = SL_enemyTP.transform.Find("Fill Area").GetChild(0).GetComponent<Image>();

        IMG_playerHP.color  = colors[(int)SliderColor.Hp_default];
        IMG_playerTP.color  = colors[(int)SliderColor.Tp_default];
        IMG_enemyHP.color   = colors[(int)SliderColor.Hp_default];
        IMG_enemyTP.color   = colors[(int)SliderColor.Tp_default];

        playerElements = new UIModElements(
            IMG_playerHP,
            IMG_playerTP,
            TF_playerHitAnchor
        );

        enemyElements = new UIModElements(
            IMG_enemyHP,
            IMG_enemyTP,
            TF_enemyHitAnchor
        );

        dict_dmgAccList.Add(Parts.Weakpoint,
                            new DmgAccText( GO_attackList.transform.GetChild(0).Find("Percentage").GetComponent<TMP_Text>(),
                                            GO_attackList.transform.GetChild(0).Find("Damage").GetComponent<TMP_Text>()));
        dict_dmgAccList.Add(Parts.Thorax,
                            new DmgAccText(GO_attackList.transform.GetChild(1).Find("Percentage").GetComponent<TMP_Text>(),
                                            GO_attackList.transform.GetChild(1).Find("Damage").GetComponent<TMP_Text>()));
        dict_dmgAccList.Add(Parts.Outer,
                            new DmgAccText(GO_attackList.transform.GetChild(2).Find("Percentage").GetComponent<TMP_Text>(),
                                            GO_attackList.transform.GetChild(2).Find("Damage").GetComponent<TMP_Text>()));


    }

    void Update()
    {
        if (!b_playerReady && !b_enemyReady) //둘다 준비상태가 아닐 때
        {
            
            //speed를 기반으로 증가량을 계산.
            //0~100까지 증가량은 속도 1.0 기준으로 3초가 걸린다.
            float tmp_playerIncrement = Time.deltaTime * f_playerSpeed * 33.3f;
            float tmp_enemyIncrement = Time.deltaTime * f_enemySpeed * 33.3f;

            //증가량만큼 더해주고
            SL_playerTP.value += tmp_playerIncrement;
            SL_enemyTP.value += tmp_enemyIncrement;

            //플레이어가 포인트가 다 채워졌으면 행동을 실행하도록 잠시 멈춘다.
            if (SL_playerTP.value >= 100)
            {
                IMG_playerTP.color = colors[(int)SliderColor.Tp_hilighted];
                b_playerReady = true;
                Debug.Log("플레이어 준비");
            }
            if (SL_enemyTP.value >= 100)
            {
                IMG_enemyTP.color = colors[(int)SliderColor.Tp_hilighted];
                b_enemyReady = true;
                Debug.Log("크리쳐 준비");
            }
        }

        if (b_playerReady) GO_actionList.SetActive(true);
        else if (b_enemyReady) BA_battleActions.Attack(false);

    }

    void StartBattleScene(Creature CR_Opponent)
    {
        BA_battleActions.CR_Enemy = CR_Opponent;

        //행동 포인트관련 초기화 : 전투 중간에 변경될 일이 있을까? => 있으면 f_Speed같은 경우는 ref로 넘겨줘야함
        SL_playerTP.value = GameManager.Instance.GetComponent<Player>().GetPlayerStats().prepareSpeed;
        f_playerSpeed = GameManager.Instance.GetComponent<Player>().GetPlayerStats().speed;

        SL_enemyHP.value = SL_enemyHP.maxValue = CR_Opponent.health;
        SL_enemyTP.value = CR_Opponent.prepareSpeed;
        f_enemySpeed = CR_Opponent.speed-0.05f;

        TMP_playerDamage.text = "DMG\n" + GameManager.Instance.GetComponent<Player>().GetPlayerStats().damage;
        TMP_EnemyDamage.text = "DMG\n" + CR_Opponent.damage;

        //전투창 활성화
        this.gameObject.SetActive(true);        
    }

    public void ChangeSliderValue(bool b_IsPlayer, StatsType statsType, float f_val)
    {
        switch (statsType)
        {
            case StatsType.Hp:
                if (b_IsPlayer) SL_playerHP.value = f_val;
                else SL_enemyHP.value = f_val;
                break;
            case StatsType.Tp:
                if (b_IsPlayer) SL_playerTP.value = f_val;
                else SL_enemyTP.value = f_val;
                break;
            case StatsType.Damage:
                break;
            case StatsType.Defense:
                break;
            case StatsType.Speed:
                break;
            default:
                break;
        }
    }

    public void EndTurn(bool b_isPlayer) {
        ChangeSliderValue(b_isPlayer, StatsType.Tp, 0);
        if (b_isPlayer)
        {
            b_playerReady = false;
            GO_actionList.SetActive(false);
            IMG_playerTP.color = colors[(int)SliderColor.Tp_default];
        } else
        {
            IMG_enemyTP.color = colors[(int)SliderColor.Tp_default];
            b_enemyReady = false;
        }
        
    }

    
}
