using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour
{
    private Unit unit;

    [SerializeField] private Image healthBar;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        
        healthBar.fillAmount = 1;
    }


    public void InitHealth()
    {
        
    }
    
    public void UpdateHealth()
    {
        float tagret = (unit.unitData.HP + 0.0f) / unit.unitData.totalHP;
        CommTween.To(healthBar.fillAmount, tagret, 0.2f, (value) =>
        {
            healthBar.fillAmount = value;
        });
    }

    private void LateUpdate()
    {
        healthBar.transform.parent.parent.forward = Camera.main.transform.forward;
    }

    private void Update()
    {
        // Vector3 viewDir = healthBar.transform.parent.position - Camera.main.transform.position;
        // healthBar.transform.parent.transform.rotation = Quaternion.LookRotation(viewDir);
    }
}
