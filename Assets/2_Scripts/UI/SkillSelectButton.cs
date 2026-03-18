using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using VContainer;

// 칼 Tilemap[tilemap_106]
// 단검 Tilemap[tilemap_103]
// 도끼 Tilemap[tilemap_119]
// 해머 Tilemap[tilemap_117]
public class SkillSelectButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Button button;
    [SerializeField] private Image skillImage;
    private int _skillId;
    private Action<int> _onClickCallback;
    // [핵심] 어드레서블 메모리 해제를 위해 핸들을 기억해둘 변수
    private AsyncOperationHandle<Sprite> _imageHandle;

    public void Init(SkillData data, SkillBase currentSkill, Action<int> onClick)
    {
        _skillId = data.id;

        nameText.text = data.name; // 스킬 이름
        int imageAddress;

        switch (data.id)
        {
            case 4001: imageAddress = 106; break;
            case 4002: imageAddress = 103; break;
            case 4003: imageAddress = 119; break;
            case 4004: imageAddress = 117; break;
            default: imageAddress = 0; break;
        }

        ImageAsyncSetting(imageAddress).Forget();

        if (currentSkill == null)
        {
            descText.text = $"Level 1\n " + "\n" +
                            $"Damage: {data.baseAtk}\n " + "\n" +
                            $"CoolTime: {data.cooldown} second";
        }
        else
        {
            CurrentData currentData = currentSkill.GetCurrentData();
            descText.text =
                $"currentLevel {currentData.level}\n" +
                $"nextLevel {currentData.level + 1}\n" + "\n" +
                $"currentDamage: {currentData.damage}\n" +
                $"nextDamage: {currentData.damage + data.atkPerLevel}\n" + "\n" +
                $"currentCoolTime: {currentData.cooldown} sec\n" +
                $"nextCoolTime: {currentData.cooldown - 0.5f} sec";
        }


        _onClickCallback = onClick;

        // 기존 리스너 제거 후 새로 달기 (풀링 재사용 대비)
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickButton);
    }

    private async UniTask ImageAsyncSetting(int address)
    {
        if (_imageHandle.IsValid())
        {
            Addressables.Release(_imageHandle);
        }

        if (address == 0)
        {
            skillImage.sprite = null;
            return;
        }

        // 2. 핸들을 저장하면서 새 이미지 로드
        _imageHandle = Addressables.LoadAssetAsync<Sprite>($"Tilemap[tilemap_{address}]");
        Sprite image = await _imageHandle.ToUniTask();

        // 3. 내가 살아있는지 확인
        if (this == null || skillImage == null) return;

        // 4. 적용
        skillImage.sprite = image;
    }

    private void OnClickButton()
    {
        _onClickCallback?.Invoke(_skillId);
    }
    // 5. [메모리 최적화] 버튼 자체가 파괴될 때도 꼭 이미지를 놔주어야 합니다.
    private void OnDestroy()
    {
        if (_imageHandle.IsValid())
        {
            Addressables.Release(_imageHandle);
        }
    }
}