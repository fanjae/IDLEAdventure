using TMPro;
using UnityEngine;

public class DpsRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;

    private BattleUnit unit;

    public void Initialize(BattleUnit unit)
    {
        this.unit = unit;
        Refresh();
    }

    public void Refresh()
    {
        if (unit == null || infoText == null) return;

        float dps = 0.0f;
        if (DpsManager.Instance != null)
        {
            dps = DpsManager.Instance.GetDps(unit);
        }

        string unitName = unit.UnitData != null ? unit.UnitData.UnitName : unit.name;
        infoText.text = $"{unitName}/DPS : {FormatDps(dps)}";
    }
    private string FormatDps(float value)
    {
        if (value >= 1000.0f) return $"{value / 1000.0f:0.##}K";
        return $"{value:0}";
    }
}
