# Fantasy Kingdom Sim - Game Rules Document

This document describes the complete game mechanics and rules for the fantasy kingdom simulation game. It covers all systems, entity anatomy, and how game elements interact without referencing specific content definitions or UI implementation details.

---

## Table of Contents

1. [World Structure](#1-world-structure)
2. [Simulation Loop](#2-simulation-loop)
3. [Entity Types Overview](#3-entity-types-overview)
4. [Creature Anatomy](#4-creature-anatomy)
5. [Building Anatomy](#5-building-anatomy)
6. [Prop Anatomy](#6-prop-anatomy)
7. [Item Anatomy](#7-item-anatomy)
8. [Condition System](#8-condition-system)
9. [Type Abilities](#9-type-abilities)
10. [Feats](#10-feats)
11. [Attribute Scores](#11-attribute-scores)
12. [Skills System](#12-skills-system)
13. [Combat System](#13-combat-system)
14. [Experience and Leveling](#14-experience-and-leveling)
15. [Behavior and AI System](#15-behavior-and-ai-system)
16. [Activity System](#16-activity-system)
17. [Harvesting System](#17-harvesting-system)
18. [Crafting System](#18-crafting-system)
19. [Planting System](#19-planting-system)
20. [Building Construction](#20-building-construction)
21. [Creature Spawning](#21-creature-spawning)
22. [Movement and Pathfinding](#22-movement-and-pathfinding)
23. [Inventory and Item Holding](#23-inventory-and-item-holding)
24. [Equipment and Outfits](#24-equipment-and-outfits)
25. [Transport Routes](#25-transport-routes)
26. [Prop Growth](#26-prop-growth)
27. [Death and Destruction](#27-death-and-destruction)
28. [Claiming System](#28-claiming-system)

---

## 1. World Structure

### Map Grid
- The world is a 2D grid-based map where each cell can contain multiple placeables
- Coordinates are represented as integer Vector2 positions (x, y)
- Distances between entities use Chebyshev distance (maximum of x-distance and y-distance)

### Placeables and Size Categories
Every entity that exists on the map is a "Placeable" with a size category:
- **Positive size category (n > 0)**: Entity occupies n x n grid cells
- **Negative size category (n < 0)**: Entity occupies a fraction (1/|n|) of a cell (multiple can share)
- **Zero size category**: Entity takes up no space

### Pathability
A cell is pathable (walkable) if it contains no:
- Buildings marked as non-pathable
- Props marked as non-pathable
- Creatures

### Buildability
A position is buildable for a building of size `s` if the area from (-1,-1) to (s+1, s+1) relative to the building position contains:
- No buildings
- No props
- No items or creatures in the interior cells (excluding the perimeter ring)

---

## 2. Simulation Loop

### Tick-Based Simulation
The game advances in discrete time units called "ticks". Each tick processes:

1. **Command Execution**: Player-issued commands are validated and executed
2. **Observe and Feel Phase**: All placeables update their internal state (conditions tick, experience checks, etc.)
3. **Think and Plan Phase**: Creatures compute their next activity, buildings spawn creatures
4. **Act Phase**: Creatures perform their current activity
5. **Destruction Check**: Destroyed entities are removed and drop items are spawned
6. **Thinking Queue Processing**: Deferred AI computations are processed (2 per tick, prioritizing player units)

### Thinking Queue
To prevent AI lag, creature activity computations are queued:
- Player team creatures (team > 0) get priority
- CPU team creatures go to standard queue
- 2 computations processed per tick

---

## 3. Entity Types Overview

The game has four main placeable entity types:

| Entity | Destructible | Can Hold Items | Can Move | Has Team |
|--------|--------------|----------------|----------|----------|
| Creature | Yes | Yes | Yes | Yes |
| Building | Yes | Yes | No | Yes |
| Prop | Yes | No | No | No |
| Item | No | No | No | No |

---

## 4. Creature Anatomy

### Identity
- **Name**: Display name for the creature
- **Team Number**: Positive = human player, Negative = CPU, Zero = neutral
- **Creature Type**: Reference to the template defining base stats

### Core Statistics

#### Attribute Scores
Six primary attributes (see [Attribute Scores](#11-attribute-scores)):
- Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma
- Base value: 8 + 1d6 (rolled at creation)
- Modified by creature type bonuses and conditions

#### Health
- **Max Health** = `startingHealth + (level * healthPerLevel) + conditionModifiers`
- **Damage Taken**: Tracks accumulated damage
- **Health Fraction** = `(maxHealth - damageTaken) / maxHealth`
- Creature is destroyed when health fraction <= 0 OR any attribute score <= 0

### Progression
- **Level**: Current level (starts at 1)
- **Experience**: Current XP (resets to 0 on level up)
- **Experience for Next Level** = `level^2`
- **Skill Increases**: Floating-point values tracking skill progression

### Abilities and Feats
- **Type Abilities**: List of abilities unlocked at specific levels
- **Feats**: List of permanent bonuses

### State
- **Home Position**: Location of home building
- **Conditions**: List of active status effects with stack counts
- **Cooldown Ticks Remaining**: Time until next action
- **Last Interrupt Tick**: Prevents interrupt spam (1000 tick cooldown)

### Behavior
- **Creature Behavior**: AI profile determining priorities
- **Current Activity**: What the creature is doing
- **Staged Activity**: Next activity awaiting promotion
- **Activity Verb**: Description of current action ("Harvesting", "Fighting", etc.)

### Derived Statistics
- **Difficulty Estimate** = `10 + level` (used for danger calculations)
- **Move Speed** = `max(1, 10 + moveSpeedModifiers)` ticks per square

---

## 5. Building Anatomy

### Identity
- **Building Type**: Reference to template
- **Team Number**: Ownership (positive = player, negative = CPU)

### Statistics
- **Max Health**: From building type
- **Size**: Grid dimensions (size x size cells)
- **Is Pathable**: Whether creatures can walk through

### Creature Support
Buildings can spawn and support creatures:
- **Supported Creature Types**: List of creature types this building can spawn
- **Supported Creature Counts**: Maximum population per type
- **Spawn Times**: Ticks required to spawn each creature type
- **Creatures By Type**: Currently supported creatures, tracked by type name
- **Spawning Creature**: Currently spawning creature (or null)
- **Spawn Ticks Remaining**: Time until spawn completes

### Item Storage
- **Requestable Item Types**: Items this building can request
- **Requestable Item Counts**: Maximum requestable amount per type
- **Requested Item Amounts**: Current request quantities (player-configurable)
- Buildings hold items in their internal inventory

### Production
- **Craftable Item Types**: Items that can be crafted here
- **Plantable Prop Types**: Props that can be planted near this building
- **Planting Zone**: Rectangular area (xMin, xMax, yMin, yMax) where planting is allowed

### Construction
- **Buildable Building Types**: What buildings can be constructed from this building
- **Construction Item Types/Amounts**: Required materials

### Rest and Healing
- **Rest Heal Amount**: HP restored per rest action
- **Rest Heal Cooldown**: Ticks between heals

### Bonus Props
Buildings can spawn bonus props at fixed positions:
- **Supported Bonus Props**: Prop types
- **Bonus Prop Positions**: X/Y offsets from building
- **Respawn Ticks**: Time until respawn after harvesting

### Combat
- **Difficulty Estimate**: Used for danger calculations
- **Demolition Difficulty**: Target number to damage this building

### Wave System
- **Wave Times**: Tick numbers when waves trigger
- **Wave Sizes**: Number of creatures per wave
- **Wave Behavior**: Behavior name to assign during waves

### Special Flags
- **Is Starting Building**: Placed at game start
- **Destroy To Win**: Destroying this building is a victory condition

---

## 6. Prop Anatomy

### Identity
- **Prop Type**: Reference to template

### Statistics
- **Max Health**: Damage threshold (default 100)
- **Size**: Grid dimensions
- **Is Pathable**: Whether creatures can walk through

### Harvesting
- **Harvesting Skill**: Required skill name
- **Harvesting Difficulty**: Target number for success rolls
- **Harvesting Required**: Total harvest points needed to deplete
- **Harvested Amount**: Accumulated harvest damage
- **Requires Implement**: Whether a tool is needed

### Items
- **Produced Items**: Items dropped when fully harvested
- **Produced Items Probabilities**: Weighted chances for each item

### Growth
- **Growth Ticks**: Time until transformation (0 = no growth)
- **Grows Into**: Prop type to transform into
- **Replaced By**: Alternative replacement prop

### State
- **Tick Created**: When this prop was placed (for growth timing)

### Derived
- **Harvested Fraction** = `harvestedAmount / harvestingRequired`
- Prop is destroyed when harvested fraction >= 1 OR damage >= 100

---

## 7. Item Anatomy

### Identity
- **Item Type**: Reference to template

### Statistics
- **Size**: Storage size category
- **Equipment Category**: Slot type (e.g., "weapon", "armor", "none")
- **Equipment Level**: Power tier for upgrading decisions

### Combat Properties
- **Weapon Damage**: Dice roll (NdM format)
- **Weapon Range**: Attack range in squares (1 = melee)
- **Weapon Skill**: Required skill to use
- **Condition Infliction**: Bonus to inflict conditions

### Utility Properties
- **Harvesting Skills**: List of skills this tool enables
- **Harvesting Amount**: Harvest points per use
- **Repair Amount**: HP restored when used for repairs

### Crafting
- **Crafting Inputs**: Required items to craft this
- **Crafting Skill**: Required skill name
- **Crafting Level**: Minimum skill level needed
- **Crafting Time**: Ticks to complete crafting

### Bonuses
- **Skill Bonus Names**: Skills modified by holding this item
- **Skill Bonus Amounts**: Modifier values
- **Condition Protection**: Defense bonus against conditions

### Conditions
- **Conditions Inflicted**: Status effects applied on hit
- **Base Stacks**: Initial stack count per condition
- **Max Stacks**: Stack ceiling per condition

### State
- **Consumed**: Whether item has been used up

---

## 8. Condition System

### Condition Type Anatomy
- **Name**: Identifier
- **Duration**: Ticks per stack decay
- **Is Harmful**: Whether this requires a roll to apply
- **Protection Attribute**: Which attribute score defends against this

### Effects
- **Ticks Per Damage**: Interval for damage-over-time (0 = no DoT)
- **Damage**: Damage per tick
- **Speed Modifier**: Multiplier to cooldown reduction (stacks)
- **Move Speed Modifier**: Flat modifier to movement ticks
- **Max Health Modifier**: Change to maximum health
- **Condition Protection**: Defense against other conditions

### Modifiers
- **Skill Bonus Names/Amounts**: Skill modifiers while active
- **Modified Attribute Scores**: Which attributes are affected
- **Attribute Modifier Amounts**: Change to each attribute

### Active Condition State
- **Condition Type**: The template
- **Stacks**: Current stack count (float for fractional decay)
- **Elapsed Ticks**: Accumulator for duration timing

### Stack Mechanics
- Stacks are added with `min(current + new, maxStacks)`
- Each tick: `elapsedTicks += floor(stacks)`
- When `elapsedTicks >= duration`: remove `elapsedTicks / duration` stacks
- Condition removed when stacks <= 0

### Speed Modifier Calculation
- Total speed modifier = `sum(conditionSpeedModifier * stacks)` for all conditions
- Cooldown reduction per tick = `max(0.1, 1 + speedModifier)`

---

## 9. Type Abilities

Type abilities define what actions a creature can perform.

### Targeting
- **Range**: Maximum distance to target (in squares, 1 = melee)
- **Effect Radius**: Area of effect around target (0 = single target)
- **Enemy Targets**: Maximum enemy targets
- **Ally Targets**: Maximum ally targets

### Timing
- **Cooldown**: Ticks added after use
- **Recharge Ticks**: Minimum ticks between uses

### Damage
- **Damage**: Dice roll for damage (NdM format, null = no damage)
- **Skill**: Skill used for hit rolls

### Healing
- **Heal**: HP restored (for ally abilities)

### Weapon Integration
- **Weapon Skills**: List of weapon skills this ability can use
- When weapon skills are specified, ability uses equipped weapon's damage/range

### Harvesting
- **Harvesting Skill**: What resource skill this enables
- **Harvesting Amount**: Points per harvest action

### Repair
- **Repair Implement Skills**: Weapon skills that count as repair tools
- **Repair Amount**: HP restored per repair action

### Crafting
- **Crafting Skill**: What crafting skill this enables
- **Crafting Skill Bonus**: Bonus to that skill

### Planting
- **Planting Skill**: What planting skill this enables
- **Planting Skill Bonus**: Bonus to that skill

### Conditions
- **Conditions Inflicted**: Status effects applied on hit/use
- **Base Stacks**: Initial stack count per condition
- **Max Stacks**: Stack ceiling per condition

### Readiness Check
An ability is "up" when:
1. `ticksSinceLastUse >= rechargeTicks`
2. If weapon skills required: creature holds a weapon matching one of them

---

## 10. Feats

Feats provide permanent passive bonuses.

### Anatomy
- **Name**: Identifier
- **Skill Bonus Name**: Skill to modify (empty = no skill bonus)
- **Skill Bonus**: Amount to add to skill checks

---

## 11. Attribute Scores

### The Six Attributes
| Attribute | Abbrev | Primary Uses |
|-----------|--------|--------------|
| Strength | STR | Melee damage, carrying |
| Dexterity | DEX | Ranged attacks, dodge, speed |
| Constitution | CON | Health, resistance |
| Intelligence | INT | Crafting, magic |
| Wisdom | WIS | Perception, willpower |
| Charisma | CHA | Social, leadership |

### Calculation
1. Base value: 8 + 1d6 (rolled at creature creation)
2. Add creature type bonus for that attribute
3. Add condition modifiers

### Attribute Modifier
`attributeModifier = floor(attributeScore / 2) - 5`

This modifier is added to skill checks based on the skill's key attribute.

### Death by Attribute Loss
If any attribute score reaches 0 or below, the creature is destroyed.

---

## 12. Skills System

### Skill-Attribute Mapping
Each skill is linked to a key attribute (defined in configuration). The attribute modifier is added to all skill checks.

### Skill Modifier Calculation
```
skillModifier = floor(skillIncreases[skill])
              + creatureTypeBonus
              + sum(featBonuses)
              + sum(heldItemBonuses)
              + sum(conditionModifiers)
              + attributeModifier(keyAttribute)
```

### Skill Increase Formula
When gaining experience from an activity:
```
increase = max(0, ln(max(encounterLevel - 8, 1))) / (currentSkillValue + 1)
```
This gives diminishing returns as skill improves.

### Special Skills
- **Crafting Skills**: Require a crafting ability with matching skill
- **Planting Skills**: Require a planting ability with matching skill
- Returns -1 if creature lacks the required ability

---

## 13. Combat System

### Attack Types
1. **Weapon Attacks**: Use equipped weapon's damage and range
2. **Non-weapon Attacks**: Use ability's damage and range directly
3. **Ally Effects**: Apply conditions to friendly targets

### Hit Resolution

#### Melee Strike
1. Attacker rolls: `d20 + skillModifier(weaponSkill)`
2. Defender blocks if: `attackRoll <= 10 + defenderSkillModifier("defense")`
3. If hit: roll weapon damage, apply to defender

#### Ranged Strike
1. Attacker rolls: `d20 + skillModifier(weaponSkill)`
2. Defender dodges if: `attackRoll <= 10 + defenderSkillModifier("dodge")`
3. If hit: roll weapon damage, apply to defender

#### Non-weapon Strike
1. Attacker rolls: `d20 + skillModifier(abilitySkill)`
2. Defender resists if: `attackRoll <= 10 + defenderSkillModifier("resistance")`
3. If hit: roll ability damage, apply to defender

### Area of Effect
For abilities with effect radius > 0:
1. Find up to `maxTargets` enemies within range
2. For each target position, affect all enemies in radius around that point

### Building Attacks
When attacking buildings:
1. Roll multiplier against demolition difficulty:
   - `d20 + skillModifier("demolition") >= difficulty` = +1 multiplier
   - Roll >= difficulty + 10 = +1 more
   - Roll >= difficulty + 20 = +1 more
   - Natural 20 = +1 more
2. Melee weapons get +2 base multiplier; non-weapon abilities get +1
3. Damage = `weaponDamage.roll() * multiplier`

### Condition Infliction

#### Against Enemies (Harmful)
1. Calculate target's protection class:
   ```
   protectionClass = 10 + targetLevel
                   + sum(conditionProtectionFromConditions)
                   + sum(conditionProtectionFromItems)
                   + attributeModifier(condition.protectionAttribute)
   ```
2. Attacker rolls: `d20 + skillModifier(attackSkill)`
3. If roll >= protectionClass: apply condition with baseStacks (capped at maxStacks)

#### On Allies (Beneficial)
- Applied automatically without roll
- Stacks still capped at maxStacks

### Target Priority
1. Creatures in range are targeted first
2. If enemy target count not met, buildings in range are targeted

---

## 14. Experience and Leveling

### Experience Gain
- Only player team creatures (team > 0) gain experience
- XP gained per action: `(encounterLevel - 9)^2`
- Encounter level varies by activity (creature difficulty, prop difficulty, etc.)

### Level Up
- Trigger: `experience >= level^2`
- On level up:
  - Increment level
  - Reset experience to 0
  - Unlock abilities that have this level as their unlock level

### Experience Sources
| Activity | Encounter Level |
|----------|-----------------|
| Killing creature | `10 + creatureLevel` |
| Harvesting prop | `harvestingDifficulty` |
| Crafting item | `craftingLevel + 10` |
| Planting prop | `plantingDifficulty` |
| Successful block | `attackRoll` that was blocked |
| Repairing | 10 |
| Demolition | `demolitionDifficulty` |

---

## 15. Behavior and AI System

### Creature Behavior Type
Defines AI priorities and parameters:

#### Priority List
Ordered list of behavior priorities (e.g., "Flee", "Fight", "Harvest Requested Items", "Rest", "Wander")

#### Range Parameters
- **Repair Range**: How far to travel for repairs
- **Drop Off Range**: Range for item delivery
- **Pick Up Range**: Range for item collection
- **Harvest Range**: Range for harvesting
- **Craft Range**: Range for crafting
- **Rest Range**: Range for resting
- **Wander Radius**: Maximum distance from home while wandering
- **Hunt Range**: Range from home to hunt
- **Hunt Look Distance**: Perception range for prey
- **Equip Range**: Range for equipment pickup
- **Plant Range**: Range for planting
- **Raid Range**: Range from home for raids
- **Raid Look Distance**: Perception range for raid targets
- **Defend Range**: Range to defend home

#### Level Difference Limits
- **Hunt Maximum/Minimum Level Difference**: Target difficulty relative to self
- **Raid Maximum/Minimum Level Difference**: Building difficulty relative to self

#### Danger Thresholds
- **Danger Look Distance**: Range to assess threats
- **Interrupt Danger Threshold**: Danger multiplier to abandon current activity
- **Flee Danger Threshold**: Danger multiplier to trigger fleeing
- **Fight Danger Threshold**: Danger multiplier to engage

#### Equipment
- **Equip Categories**: Equipment slots this behavior tries to fill

### Danger Rating Calculation
```
dangerRating = sum(enemyDifficultyEstimates) - sum(allyDifficultyEstimates)
```
For all creatures within danger look distance.

### Interrupt Check
If `dangerRating > selfEncounterLevel * interruptDangerThreshold`:
- Abandon current activity
- 1000 tick cooldown before next interrupt

### Activity Selection
Process priorities in order until one returns an activity:
1. Each priority evaluates conditions and returns an activity or null
2. First non-null activity is selected
3. Activities are "staged" pending claim verification

---

## 16. Activity System

### Activity Base Properties
- **Encounter Level**: XP source level
- **Source Placeable**: Target entity (or null)
- **Position**: Target location (if no placeable)
- **Proximity Requirement**: How close creature must be to act

### Activity Lifecycle
1. **Selection**: Behavior selects activity during Think phase
2. **Staging**: Activity placed in staged slot
3. **Claiming**: Target placeables marked as claimed
4. **Movement**: Creature moves toward target if too far
5. **Performance**: Activity.Perform() called when in range
6. **Completion**: Activity checked for completion/impossibility

### Activity Types

#### Harvest Activity
- Target: Prop
- Proximity: 1 square
- Claims: Target prop
- Action: `creature.HarvestProp(prop)`
- Complete when: Prop destroyed or creature can't harvest

#### Hunt Activity
- Target: Enemy creature
- Proximity: Weapon/ability range
- Claims: None (multiple can hunt same target)
- Action: `creature.UseAnyAbility()`
- Complete when: Target destroyed

#### Raid Activity
- Target: Enemy building
- Proximity: Weapon/ability range
- Claims: None
- Action: `creature.UseAnyAbility()`
- Complete when: Target destroyed

#### Craft Activity
- Target: Workshop building
- Proximity: 1 square
- Claims: Workshop
- Action: Consume inputs, create output item
- Complete when: Item crafted, inputs missing, or skill insufficient

#### Pick Up Activity
- Target: Item
- Proximity: 1 square
- Claims: Item
- Action: Move item to creature's inventory
- Option: Add to outfit (equipment)
- Complete when: Item picked up or no longer available

#### Deliver Activity
- Target: Item + destination building
- Proximity: 1 square to current target
- Claims: Item
- Action:
  - If not holding item: pick up
  - If holding item: transfer to building
- Complete when: Item delivered or no longer needed

#### Repair Activity
- Target: Damaged friendly building
- Proximity: 1 square
- Claims: Building
- Action: `creature.RepairBuilding(building)`
- Complete when: Building at full health or destroyed

#### Rest Activity
- Target: Friendly building
- Proximity: 1 square
- Claims: None (multiple can rest)
- Action: Heal creature, add cooldown
- Complete when: Creature at full health and adjacent to building

#### Plant Activity
- Target: Position
- Proximity: 1 square
- Claims: None
- Action: `creature.PlantProp(propType, position)`
- Complete when: Position occupied or skill insufficient

#### Idle Activity
- Target: Current position
- Proximity: 0
- Claims: None
- Action: Nothing
- Complete when: 150 ticks elapsed

#### Wander Activity
- Target: Home building
- Proximity: Infinite (always in range)
- Claims: None
- Action: 1% chance per tick to move randomly within wander radius
- Complete when: 150 ticks elapsed or home destroyed

---

## 17. Harvesting System

### Harvesting Ability Check
Creature can harvest a prop if:
1. Has ability with matching harvesting skill
2. If prop requires implement: holds item with matching harvesting skill

### Cooldown and Amount
For each matching ability:
- Compare `harvestAmount / cooldown` ratios
- Select best efficiency
- With implements: use item's harvest amount, ability's cooldown

### Harvest Action
1. Set cooldown to ability's cooldown
2. Roll: `d20 + skillModifier(harvestingSkill)`
3. If `roll >= harvestingDifficulty` OR natural 20:
   - Add harvest amount to prop's harvested total
   - If prop destroyed: gain experience

### Prop Item Drop
When harvested to completion:
1. Build weighted list from produced items and probabilities
2. Select random item from weighted list
3. Drop item at prop's location

---

## 18. Crafting System

### Crafting Requirements
1. Workshop building has the item type in its craftable list
2. Building is missing the item (requested > stored)
3. All input materials present in workshop
4. Creature has crafting ability matching item's crafting skill
5. `creature.craftingSkillModifier >= item.craftingLevel`

### Crafting Action
1. Consume input items (prefer creature's inventory, then workshop)
2. Create output item in workshop inventory
3. Gain experience: `craftingLevel + 10`
4. Add cooldown: `item.craftingTime`

---

## 19. Planting System

### Planting Requirements
1. Building has prop type in plantable list
2. Position within planting zone (xMin to xMax, yMin to yMax from building)
3. Position coordinates both even (for spacing)
4. No prop or building at position
5. Creature has planting ability matching prop's planting skill
6. `creature.plantingSkillModifier >= prop.plantingLevel`

### Planting Action
1. Set cooldown to ability's cooldown
2. Roll: `d20 + plantingSkillModifier(skill)`
3. If `roll >= plantingDifficulty` OR natural 20:
   - Create prop at position
   - Gain experience

---

## 20. Building Construction

### Construction Requirements
1. Source building has target building type in buildable list
2. Source building contains all required construction materials
3. Target position is buildable

### Construction Action
1. Consume construction materials from source building
2. Create new building at target position
3. New building starts at 10% health (90% damage taken)
4. Building must be repaired to full functionality

---

## 21. Creature Spawning

### Spawn Requirements
1. Building supports the creature type
2. Current count < maximum for that type
3. Building not already spawning
4. All spawn cost items present in building

### Spawn Process
1. Consume spawn cost items
2. Create creature with home at building position
3. Add to building's creature roster
4. Set spawn timer to creature type's spawn time

### Spawn Completion
When timer reaches 0:
1. Place creature adjacent to building (offset -1, -1)
2. Add starting equipment to creature (held + outfit)
3. Clear spawning creature reference

### CPU Auto-Spawn
CPU buildings (team < 0) automatically spawn when:
- Not currently spawning
- Any supported creature type below max count

---

## 22. Movement and Pathfinding

### Movement Cost
- Base move speed: 10 ticks per square
- Modified by conditions: `max(1, 10 + moveSpeedModifiers)`
- Moving sets cooldown to move speed

### Pathfinding Algorithm
A* algorithm with:
- 8-directional movement (including diagonals)
- Manhattan distance heuristic
- Maximum 1000 iterations
- Returns next step toward target

### Pathfinding Rules
- Intermediate cells must be pathable
- Target cell doesn't need to be pathable
- If no path found: try clockwise blind movement

### Blind Movement Fallback
When A* fails:
1. Determine general direction to target
2. Try each direction clockwise from ideal
3. Take first pathable step found
4. If all blocked: abandon activity

---

## 23. Inventory and Item Holding

### Holding Mechanics
- Creatures and buildings can hold items
- Items can be transferred between holders
- Items in inventory are removed from map grid

### Pick Up Rules
- Holder and item must be within distance 1
- Item removed from grid, added to holder's inventory

### Transfer Rules
- Cannot transfer items that are part of creature's outfit
- Direct holder-to-holder transfer

### Drop Rules
- Item removed from holder's inventory
- Item placed at holder's grid position

---

## 24. Equipment and Outfits

### Outfit Definition
A creature's "outfit" is the subset of held items that are actively equipped.

### Equipment Properties
- **Equipment Category**: Slot type (weapon, armor, etc.)
- **Equipment Level**: Power tier

### Equipping
When picking up with `addToOutfit = true`:
1. Add item to creature's held items
2. Mark item as part of outfit

### Equipment Benefits
- Outfit items contribute skill bonuses
- Outfit items contribute condition protection
- Weapons in outfit used for attacks

### Upgrade Logic
Creatures seek equipment upgrades when:
1. Item has equipment category in behavior's equip list
2. Item's equipment level > current level for that category
3. Item not claimed and not held by another creature

---

## 25. Transport Routes

### Route Definition
A directed edge from source building to destination building.

### Route Effects
When delivering items:
- Items in buildings with transport route to requestor can be taken
- Even if source building hasn't exceeded its request amount

### Route Creation
Player command creates route between two buildings.

### Route Removal
Routes automatically removed when either building is destroyed.

---

## 26. Prop Growth

### Growth Mechanics
Props with `growthTicks > 0` transform over time.

### Growth Check (per tick)
If `currentTick - tickCreated >= growthTicks`:
1. Remove current prop
2. Create new prop of `growsInto` type at same position

### Replacement on Harvest
When prop is harvested to completion:
- If `replacedBy` defined: create that prop type at location
- This allows renewable resources (e.g., tree → stump, stump grows back into tree)

---

## 27. Death and Destruction

### Creature Death
Triggered when:
- Health fraction <= 0, OR
- Any attribute score <= 0

On death:
1. Roll item drop from creature type's drop table
2. Weight by probabilities, include "nothing" probability
3. Drop item at creature's position (if any)

### Building Destruction
Triggered when health fraction <= 0.

On destruction:
1. All supported creatures lose their home (home position = null)
2. All transport routes involving this building removed

### Prop Destruction
Triggered when:
- Health fraction <= 0, OR
- Harvested fraction >= 1

On destruction (if harvested):
1. Roll item drop from produced items table
2. Place item at prop's position
3. If `replacedBy` defined: create replacement prop

---

## 28. Claiming System

### Purpose
Prevents multiple creatures from targeting the same resource.

### Claim Rules
- Props can be claimed (for harvesting)
- Items can be claimed (for pickup/delivery)
- Buildings can be claimed (for repair, crafting)
- Creatures are NOT claimed (multiple can hunt same target)

### Claim Lifecycle
1. When activity promoted: `TryClaimPlaceables()`
2. If target already claimed: activity rejected
3. When activity ends: `UnclaimPlaceables()`

### Claim Exceptions
Some activities don't claim:
- Hunt Activity (combat free-for-all)
- Raid Activity (building attacks)
- Rest Activity (shared healing)
- Idle/Wander Activities (no target)

---

## Appendix A: Dice Notation

Damage and rolls use standard dice notation: **NdM**
- N = number of dice
- M = sides per die

### Roll Calculation
`result = random(N, N*M + 1)` (uniform distribution from N to N*M inclusive)

### Expected Value
`expectedValue = (M + 1) * N / 2`

---

## Appendix B: Key Formulas Summary

| Formula | Description |
|---------|-------------|
| `maxHealth = startingHealth + level * healthPerLevel + modifiers` | Creature max HP |
| `difficulty = 10 + level` | Creature encounter level |
| `xpNeeded = level^2` | XP for next level |
| `skillMod = increases + typeBonus + feats + items + conditions + attrMod` | Total skill modifier |
| `attrMod = floor(attrScore / 2) - 5` | Attribute to modifier |
| `moveSpeed = max(1, 10 + modifiers)` | Ticks per square |
| `cooldownReduction = max(0.1, 1 + speedMod)` | Per-tick cooldown decay |
| `protectionClass = 10 + level + protection bonuses + attrMod` | Condition resistance |

---

## Appendix C: Behavior Priority Reference

Common behavior priorities and their purposes:

| Priority | Purpose |
|----------|---------|
| Repair Damaged Buildings | Maintain base infrastructure |
| Deliver Requested Items | Fulfill building requests |
| Harvest Requested Items | Gather resources for requests |
| Craft Items | Produce requested crafted goods |
| Rest | Heal damage, return to base |
| Idle | Do nothing (fallback) |
| Wander | Random movement near home |
| Hunt | Attack nearby enemies |
| Equip | Pick up equipment upgrades |
| Flee | Retreat when danger too high |
| Fight | Engage when danger present but manageable |
| Plant Props | Grow resources |
| Raid | Attack enemy buildings |
| Defend | Protect home from invaders |
