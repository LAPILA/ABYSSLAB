//==============================================================================
// TimeManager
// - 현실 1초당 게임 내 분 수 누적 (가상 시계)
// - 낮/밤 페이즈 타이머 진행 & UI 갱신 이벤트 (OnTimeUpdate)
// - 페이즈 종료 시 OnDayEnd / OnNightEnd 이벤트 발생
//==============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using ALWeapon;
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
    public WeaponBase currentWeapon;

    [BoxGroup("Weapons")]
    [Required]
    public List<WeaponBase> weaponList;
    
    // 전역적 무기 시스템 사용 가능 여부 플래그 ( 컷신이나 기타 등등 블락용 )
    // 이 있어야 한다고 생각은 하지만 자체 지원 enable 관련 플래그 유무에 따라 다를 듯
    //private bool canUseWeaponSys = true; { get; set; }
    #endregion

    /* ======================================================================= */
    #region ▶ 무기 관련 플레이어 액션
    /* ======================================================================= */

    public event Action<WeaponBase> OnSwitchWeapon;
    //EventHandler<WeaponBase> ?

    public void SwitchWeapon(int idx)
    {
        if (idx >= 0 && idx < weaponList.Count)
        {
            currentWeapon = weaponList[idx];
            
            // OnSwitchWeapon 에 UI 관련 함수들이라던가 바인드 하여 사용 등
            OnSwitchWeapon?.Invoke(currentWeapon);
        }
    }

    // Fire 관련 이벤트

    //public event Action PreWeaponFire;
    public event Action OnWeaponFire;
    //public event Action OnWeaponFireFailed;
    // PC로부터 콜 받아서 실행 : 편의 용도
    public bool FireCurrentWeapon(/*레이트레이스용 정보 (현재 위치 시선 등)*/)
    {
        if (currentWeapon?.CanFireWeapon() != true)
        {
            //OnWeaponFireFailed?.Invoke();
            return false;
        }

        currentWeapon.PreFire();
        // PreWeaponFire?.Invoke();

        bool FireRes = currentWeapon.Fire(/*레이트레이스용 정보 (현재 위치 시선 등)*/);
        if (FireRes) OnWeaponFire?.Invoke();
        //else OnWeaponFireFailed?.Invoke();
        return FireRes;
    }


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

        // MEMO :: weaponList[0] 으로 currentWeapon 초기화... 해야할듯 한데 타이밍이 맞는가?
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {

    }

    #endregion
    /* ======================================================================= */
}
