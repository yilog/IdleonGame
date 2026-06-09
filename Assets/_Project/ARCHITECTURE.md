# Project Architecture

## Core scripts

- `Core/GameManager`: global flow and game state.
- `Core/SceneLoader`: scene and map transition entry point.
- `Core/GameContext`: shared runtime references.
- `Core/SaveManager`: reserved save/load entry point.

## Input

- `Input/PlayerInputReader`: reads player input.
- `Input/InputCommand`: normalized command model for manual input and future auto-navigation.

## Player

- `Player/PlayerController`: coordinates player modules.
- `Player/PlayerMovement`: horizontal movement, jumping, ground checks.
- `Player/PlayerClimb`: rope and ladder interaction.
- `Player/PlayerAttack`: basic attack trigger and animation events.
- `Player/PlayerStateMachine`: idle, run, jump, fall, attack, climb states.
- `Player/PlayerAnimator`: Animator parameter adapter.

## Character

- `Character/CharacterStats`: shared character attributes.
- `Character/CharacterMotor2D`: reusable 2D motor.
- `Character/Health`: health, damage, death.
- `Character/Damageable`: damage receiver contract.
- `Character/Hitbox`: attack volume.
- `Character/Hurtbox`: damage receiving volume.

## Combat

- `Combat/AttackDefinition`: ScriptableObject attack data.
- `Combat/AttackExecutor`: executes attacks from definitions.
- `Combat/DamageInfo`: damage payload.
- `Combat/CombatResolver`: final damage calculation.

## Map

- `Map/MapManager`: map-local registry and bounds.
- `Map/MapPortal`: portal from one screen scene to another.
- `Map/SpawnPoint`: target spawn positions.
- `Map/RopeArea`: rope or ladder detection area.
- `Map/MapBounds`: player and camera boundaries.

## Navigation reservation

- `Navigation/NavigationAgent2D`: future auto-navigation driver.
- `Navigation/NavigationGraph`: walk, jump, rope, and portal graph.
- `Navigation/NavigationNode`: map navigation point.
- `Navigation/NavigationLink`: connection between navigation nodes.
- `Navigation/AutoMoveController`: converts paths to input commands.

## Jobs reservation

- `Jobs/JobDefinition`: ScriptableObject job data for warrior, archer, mage.
- `Jobs/JobController`: active job and available skills.
- `Jobs/SkillDefinition`: ScriptableObject skill data.
- `Jobs/SkillExecutor`: skill execution entry point.
- `Jobs/StatModifier`: job, equipment, and buff stat changes.
