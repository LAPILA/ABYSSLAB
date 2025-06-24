using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "ABYSS_LAB/Creature/WeaperData", fileName = "WeaperData")]
public class WeaperData : ScriptableObject
{
    [BoxGroup("Basic ID", showLabel: false)]
    [LabelText("���� �ڵ�")] public string id = "MON_T-01";
    [LabelText("ǥ���")] public string displayName = "Weaper";
    [PreviewField(64), HideLabel] public Sprite cardIcon;

    [Title("Stats ��"), MinValue(1)] public float maxHP = 50;
    [MinValue(0)] public float maxStress = 100;
    [MinValue(0)] public float maxHungry = 100;

    [Title("�Ҹ� �ӵ�"), MinValue(0.1f)] public float stressDrainPerSec = 0.15f;
    [MinValue(0.1f)] public float hungryDrainPerSec = 0.25f;

    [Title("����"), MinValue(0)] public float sightRange = 3;
    [MinValue(0)] public float hearingRange = 5;

    [Title("���")]
    [Tooltip("�ٴ� ���� ��Į �±�")] public string decalTag = "DirtyDecal";
    [Tooltip("�� �۱� �ð�(��)")] public float cleanDuration = 2;

    [Title("VFX / SFX")]
    public AudioClip lickSfx;
    public GameObject vomitVfxPrefab;
}
