using System;

public enum ENUM_PATTERN_ORIGIN
{
	PLAYER,
	LEFT,
	RIGHT,
	UP,
	DOWN
}

public enum ENUM_DIRECTION
{
	PLAYER,
	UP,
	DOWN,
	RIGHT,
	LEFT,
	RANDOM_4,
}

public enum ENUM_PATTERN_BEHAVIOUR
{
	MOVING_WALL,
	CHARGER,
	HORDE,
	STAMPEDE,
	ONSCREEN,
	SUICIDE,
	ZIGZAG,
	ROTATE,
}

public enum ENUM_PLAYERWEAPON_TYPE
{
	MAIN_WEAPON,
	SHARED_WEAPON,
	SUPPORT_ITEM,
	FOOD_ITEM
}

public enum ENUM_ENEMY_MOVEMENT
{
	DEFAULT,
	ROTATE,
	DASH,
	DROP,
	SHOOTING,
	ZIGZAG,
	KEEP_DISTANCE,
}

public enum ENUM_ITEM_GATCH
{
	GOLD,
	GEM,
	CHARACTER
}

public enum ENUM_MODIFIER_TYPE
{
	ATTACK, 
	MAX_HEALTH, 
	MOVEMENT_SPEED, 
	CRIT_CHANCE, 
	CRIT_DAMAGE,
	NULLIFY_LAYER,
	DAMAGE_OVER_TIME,
	STUN,
	SHIELD,
	SHIELD_DAMAGE_OVER_TIME,
	MARK,
	EXPLODE,
	HEAL_ATTACKER,
	ATTACK_SPEED,
	INVULNERABILITY,
	GUTS,
	AREA_MULTIPLIER,
	DAMAGE_BUFF,
	DAMAGE_TAKEN,
	AGILITY,
	COOLDOWN_TIME,
	RESIST_PHYSICAL,
	RESIST_MAGICAL,
	RESIST_ALCHEMY,
	HEALING_OVER_TIME,
	SHIELD_HEALING_OVER_TIME,
	HEAL_AMPLIFIER,
	DAMAGE_OVER_TIME_AMPLIFIER,
	
}

public enum ENUM_REFRESH_TYPE
{
	Independent, 
	Ignore, 
	Refresh, 
	StackingValue, 
	// StackingDuration,
	// Destroyer,
}

public enum EnumBillboardTargetType
{
	SpriteRenderer,
	MeshRenderer
}

public enum ENUM_DAMAGE_TYPE
{
	None,
	Healing,
	Shield = 10,
	ShieldHealing = 11,
	Slash = 21,
	Pierce = 22,
	Hit = 23,
}

public enum ENUM_GAME_STATE
{
	Playing,
	Losing,
	Winning,
}

public enum ENUM_COLLIDER_SHAPE 
{
	Circle, 
	Rectangle, 
}

[Flags]
public enum ENUM_COLLIDER_LAYER
{
	None = 0,
	Player = 1,
	PlayerBullet = 2,
	Enemy = 4,
	Wall = 8,
}


