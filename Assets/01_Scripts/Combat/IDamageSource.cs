public interface IDamageSource
{
    DamageData GenerateDamageData(BaseEntity target);
    BaseEntity GetOwner();
    BaseWeapon GetWeapon();
    bool CanDamage(BaseEntity target);
}
