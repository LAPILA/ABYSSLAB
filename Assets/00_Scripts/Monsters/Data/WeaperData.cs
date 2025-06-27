using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "ABYSS_LAB/Creature/WeaperData", fileName = "WeaperData")]
public class WeaperData : ScriptableObject
{
    [BoxGroup("Basic ID", showLabel: false)]
    [LabelText("몬스터 코드")] public string id = "MON_T-01";
    [LabelText("표기명")] public string displayName = "Weaper";
    [PreviewField(64), HideLabel] public Sprite cardIcon;

    [Title("Stats ★"), MinValue(1)] public float maxHP = 50;
    [MinValue(0)] public float maxStress = 100;
    [MinValue(0)] public float maxHungry = 100;

    [Title("소모 속도"), MinValue(0.1f)] public float stressDrainPerSec = 0.15f;
    [MinValue(0.1f)] public float hungryDrainPerSec = 0.25f;

    [Title("감지"), MinValue(0)] public float sightRange = 3;
    [MinValue(0)] public float hearingRange = 5;

    [Title("기믹")]
    [Tooltip("바닥 오염 데칼 태그")] public string decalTag = "DirtyDecal";
    [Tooltip("혀 닦기 시간(초)")] public float cleanDuration = 2;

    [Title("VFX / SFX")]
    public AudioClip lickSfx;
    public GameObject vomitVfxPrefab;
}
