//==============================================================================
// TimeManager
// - 현실 1초당 게임 내 분 수 누적 (가상 시계)
// - 낮/밤 페이즈 타이머 진행 & UI 갱신 이벤트 (OnTimeUpdate)
// - 페이즈 종료 시 OnDayEnd / OnNightEnd 이벤트 발생
//==============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALWeapon;

using RaycastPro;
using RaycastPro.Casters;
using RaycastPro.RaySensors;
using TMPro;

using UnityEngine.UI;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-90)]
public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    /* ======================================================================= */
    #region ▶ Props
    /* ======================================================================= */
    [BoxGroup("Weapons")]
    [ReadOnly, ShowInInspector]
    public GameObject currentWeapon;

    [BoxGroup("Weapons")]
    [Required]
    public List<GameObject> weaponList;
    
    public AdvanceCaster waterGun;
    public AdvanceCaster flameGun;
    public RaySensor elecGun;

    [SerializeField] private TextMeshProUGUI ammoText;

    // 전역적 무기 시스템 사용 가능 여부 플래그 ( 컷신이나 기타 등등 블락용 )
    // 이 있어야 한다고 생각은 하지만 자체 지원 enable 관련 플래그 유무에 따라 다를 듯
    //private bool canUseWeaponSys = true; { get; set; }
    #endregion

    /* ======================================================================= */
    #region ▶ 무기 관련 플레이어 액션
    /* ======================================================================= */

    public event Action<GameObject> OnSwitchWeapon;
    //EventHandler<GameObject> ?

    public void SwitchWeapon(int idx)
    {
        if (idx >= 0 && idx < weaponList.Count)
        {
            currentWeapon = weaponList[idx];
            
            // OnSwitchWeapon 에 UI 관련 함수들이라던가 바인드 하여 사용 등
            OnSwitchWeapon?.Invoke(currentWeapon);
        }
    }

    /// ========= LEGACY ========= ///

    // Fire 관련 이벤트

    //public event Action PreWeaponFire;
    public event Action OnWeaponFire;
    //public event Action OnWeaponFireFailed;

    // PC로부터 콜 받아서 실행 : 편의 용도
    //public bool FireCurrentWeapon(/*레이트레이스용 정보 (현재 위치 시선 등)*/)
    //{
        //if (currentWeapon?.CanFireWeapon() != true)
        //{
        //    //OnWeaponFireFailed?.Invoke();
        //    return false;
        //}

        //currentWeapon.PreFire();
        // PreWeaponFire?.Invoke();

        //bool FireRes = currentWeapon.Fire(/*레이트레이스용 정보 (현재 위치 시선 등)*/);
        //if (FireRes) OnWeaponFire?.Invoke();
        ////else OnWeaponFireFailed?.Invoke();
        //return FireRes;
    //}

    /// ============================ ///
    
    #endregion
    /* ======================================================================= */
    #region ▶ 유니티 생명주기
    /* ======================================================================= */

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);


    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {

    }

    private void Start()
    {
        // MEMO :: weaponList[0] 으로 currentWeapon 초기화... 해야할듯 한데 타이밍이 맞는가?
        currentWeapon = weaponList[0];

        // Ammo 관련 정보 -> 라이브러리 만들고 일종의 헬퍼 함수 생성.  AdvanceCaster / WaveRay 등 Ammo 정보 가질만한 것들 인자로 받아서 캐패시티 같은 정보 빼내는 함수 구현 +
        // 스위칭 할때마다 리스너 클린업하고 재지정을 하는 식으로 할것인지...

        // 아래처럼 딱딱하게 미리 다 박아 넣는 형태도 있을것...

        waterGun.onRate.AddListener(() =>
        {
            ammoText.text = $"{waterGun.ammo.MagazineAmount} / {waterGun.ammo.MagazineCapacity}";
        });
        flameGun.onRate.AddListener(() =>
        {
            ammoText.text = $"{flameGun.ammo.MagazineAmount} / {flameGun.ammo.MagazineCapacity}";
        });
        elecGun.onCast.AddListener(() =>
        {
        //    ammoFill.fillAmount = elecGun.Influence;
        });
    }

    #endregion
    /* ======================================================================= */
}
