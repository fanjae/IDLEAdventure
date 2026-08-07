using System;

// 게임 전체에서 사용할 InventoryController 인스턴스 관리
public sealed class InventoryManager : Singleton<InventoryManager>
{
    // 실제 인벤토리 기능 처리할 컨트롤러
    private InventoryController controller;

    // 외부에서 사용할 Controller 반환
    public InventoryController Controller
    {
        get
        {
            if (controller == null)
            {
                throw new InvalidOperationException("InventoryManager가 초기화되지 않았습니다.");
            }

            return controller;
        }
    }

    // 초기화 체크
    public bool IsInitialized => controller != null;

    // ItemDatabaseSO를 이용해 게임에서 사용할 InventoryController 생성
    public void Initialize(ItemDatabaseSO itemDatabase)
    {
        if (itemDatabase == null)
        {
            throw new ArgumentNullException(nameof(itemDatabase));
        }

        if (IsInitialized)
        {
            return;
        }

        controller = new InventoryController(itemDatabase);
    }
}