/*****************************************************************************
* Project : Isors Tower Prototype
* File    : DamageType.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Defines the different types of damage that can be used by shared combat,
* health, environment, and status systems.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

/// <summary>
/// Represents the type or source category of damage applied to a damageable object.
/// </summary>
public enum DamageType
{
    Physical, 
    Starvation,
}