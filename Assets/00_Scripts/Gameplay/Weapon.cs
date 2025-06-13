//==============================================================================
// Weapon (Abst)
// - Base of variable weapons
//==============================================================================

using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

// ====== DEPRECATED ======
namespace ALWeapon
{
    [DefaultExecutionOrder(-90)]
    public abstract class WeaponBase : MonoBehaviour
    {
        /* ======================================================================= */
        #region ▶ 무기 기본 속성
        /* ======================================================================= */
        [BoxGroup("Weapon Properties")]
        public byte weaponType; // Enum?
        public float damage;
        public float cooldown;
        [ReadOnly, ShowInInspector]
        private bool cdFlag = true;

        // MEMO :: UI 관련 속성 ? 아이콘 등등

        #endregion

        /* ======================================================================= */
        #region ▶ 유니티 생명주기
        /* ======================================================================= */

        private void Awake()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {

        }

        #endregion


        /* ======================================================================= */
        #region ▶ 작동 로직
        /* ======================================================================= */

        // 무기별 발사 가능 여부 체크 ( 일단은 내부 정보만으로 )
        public virtual bool CanFireWeapon() { return cdFlag; }

        // 무기 발사 전처리부 실행 ( 필요없으면 삭제 해도? )
        public virtual void PreFire() { }

        // 일반적인 무기의 발사 실행
        public bool Fire/*_Hitscan*/()
        {
            if (!CanFireWeapon())   return false;

            // HitResult HitRes; IDamageable TargetObj;
            // 레이트레이스 실행 ( PC, Cam 정보 ) 후 HitRes TargetObj에 기록

            // 분기별 처리 생각해보기 ( 발사 가능하지만 트레이스 범위 내 노 타겟 등 )

            ActivateWeapon(/*HitRes, TargetObj*/);
            StartCoroutine(FireCooldown());

            return true;
        }

        // 무기별 수행해야 할 실질적인 이펙트 부분
        public abstract void ActivateWeapon(/*HitResult HitRes, IDamageable TargetObj*/);

        IEnumerator FireCooldown()
        {
            cdFlag = false;
            yield return new WaitForSeconds(cooldown);
            cdFlag = true;
        }

        #endregion
    }


    // 이하 별개 파일로 분리?


    [DefaultExecutionOrder(-91)]
    public class WaterWeapon : WeaponBase
    {
        public override void ActivateWeapon(/*HitResult HitRes, IDamageable TargetObj*/) { }
    }

    [DefaultExecutionOrder(-91)]
    public class ElecWeapon : WeaponBase
    {
        public override void ActivateWeapon(/*HitResult HitRes, IDamageable TargetObj*/) { }
    }

    [DefaultExecutionOrder(-91)]
    public class FlameWeapon : WeaponBase
    {
        public override void ActivateWeapon(/*HitResult HitRes, IDamageable TargetObj*/) { }
    }


    /// <summary>
    /// 웨폰에 영향 받을 수 있는 개체들에 대한 인터페이스
    /// Receive 함수를 통해 
    /// </summary>
    public interface IDamageable
    {
        void Receive(/*Object Source*/);
    }
}