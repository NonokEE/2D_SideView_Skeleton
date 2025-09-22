[System.Serializable]
public struct DamageData
{
    public BaseEntity attacker;
    public BaseEntity target;
    public IDamageSource damageSource;
    public int damage;
    
    // 확장용 (향후 추가)
    // public DamageType damageType;
    // public bool isCritical;
}
