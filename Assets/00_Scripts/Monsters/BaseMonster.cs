using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine.AI;

public enum MonsterDisposition { Friendly, Neutral, Hostile }
public interface IHitReceiver { void ApplyHit(float dmg, string dmgType = null); }
public interface IFeedReceiver { void ReceiveFood(float nutrition); }

[DisallowMultipleComponent]
#if ODIN_INSPECTOR
[HideMonoScript]
#endif
public class BaseMonster : MonoBehaviour, IHitReceiver, IFeedReceiver 
{
    /*─────────────── 1. ID / 분류 ───────────────*/
#if ODIN_INSPECTOR
    [TitleGroup("Identity", order: 0)]
    [HorizontalGroup("Identity/Split", width: 60)]
#endif
    [LabelText("Monster ID"), ReadOnly] public int id = 0;

#if ODIN_INSPECTOR
    [HorizontalGroup("Identity/Split"), LabelWidth(90)]
#endif
    public MonsterDisposition Disposition = MonsterDisposition.Neutral;

#if ODIN_INSPECTOR
    [FoldoutGroup("Identity/Extra"), TextArea, HideLabel]
#endif
    public string bio;

    /*─────────────── 2. 스탯 / 체력 ───────────────*/
#if ODIN_INSPECTOR
    [TitleGroup("Vitals", order: 10), LabelWidth(95)]
#endif
    public float maxHP = 100;
    public float maxStress = 100;

#if ODIN_INSPECTOR
    [ProgressBar(0, "maxHP", ColorGetter = "HPColor")]
#endif
    public float HP = 100;

#if ODIN_INSPECTOR
    [ProgressBar(0, "maxStress")]
#endif
    public float Stress = 0;

    /*─────────────── 3. 감지 범위 ───────────────*/
#if ODIN_INSPECTOR
    [TitleGroup("Sense", order: 15), LabelWidth(90)]
#endif
    public float SightRange = 5;
    public float HearingRange = 5;

    /*─────────────── 4. 컴포넌트 캐시 ───────────────*/
#if ODIN_INSPECTOR
    [TitleGroup("Components", order: 20)]
#endif
    [ReadOnly] public NavMeshAgent Agent;
    [ReadOnly] public Animator Anim;

    /*─────────────── 5. 이벤트 ───────────────*/
    public event System.Action<int> OnDie;
    public event System.Action<int, float> OnHPChanged;
    public event System.Action<int, float> OnStressChanged;

    /*─────────────── 6. 프로퍼티 ───────────────*/
    public bool IsDead => HP <= 0f;
    public float StressNormalized => maxStress <= 0 ? 0 : Stress / maxStress;

    /*───────────────────────────────────────────*/
    #region ▶ Unity Lifecycle
    /*───────────────────────────────────────────*/
    protected virtual void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponentInChildren<Animator>();
    }
    #endregion

    /*───────────────────────────────────────────*/
    #region ▶ Public Helpers
    /*───────────────────────────────────────────*/
    /// <summary>파생 클래스에서 초기 스탯 세팅용</summary>
    protected void InitStats(float hpMax, float stressMax)
    {
        maxHP = hpMax;
        maxStress = stressMax;
        HP = maxHP;
        Stress = 0;
    }

    /// <summary>프레임 공통 Tick – 자연/사망 처리</summary>
    protected void CommonTick()
    {
        if (IsDead) return;

        OnHPChanged?.Invoke(id, HP);
        OnStressChanged?.Invoke(id, Stress);

        if (HP <= 0)
        {
            OnDie?.Invoke(id);
            PlayAnim("Die");
        }
    }

    /*──── IHit / IFeed 기본구현 ────*/
    public virtual void ApplyHit(float dmg, string dmgType = null)
    {
        if (IsDead) return;
        HP = Mathf.Max(0, HP - dmg);
        Stress = Mathf.Min(maxStress, Stress + dmg * 0.5f);
    }

    public virtual void ReceiveFood(float nutrition) => GainCalm(nutrition * .25f);

    /*──── 공용 유틸 ────*/
    protected void GainCalm(float v) => Stress = Mathf.Max(0, Stress - v);

    /// <summary>애니메이터에 클립 호출(없으면 Debug)</summary>
    protected void PlayAnim(string clip)
    {
        if (Anim && !string.IsNullOrEmpty(clip)) Anim.Play(clip);
        else Debug.Log($"[Anim] {name} ▶ {clip}");
    }
#if ODIN_INSPECTOR
    private Color HPColor(float val) => Color.Lerp(Color.red, Color.green, val / maxHP);
#endif
    #endregion
}
