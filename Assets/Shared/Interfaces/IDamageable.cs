/*****************************************************************************
* Project : Isors Tower Prototype
* File    : IDamageable.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Defines a damage contract for objects that can receive damage.
* Implementing classes must provide damage handling based on a damage amount
* and a specific DamageType.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

/// <summary>
/// Provides a contract for objects that can receive typed damage.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Applies damage with a specific damage type to the implementing object.
    /// </summary>
    /// <param name="damage">The amount of damage to apply.</param>
    /// <param name="damageType">The type of damage being applied.</param>
    public void TakeDamage(int damage, DamageType damageType);
}