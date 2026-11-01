# Chicken Climb (Level 3, platformer) - logicarius

Unity 6000.3.26f1 (Universal 2D). Open `Level3_Platformer` in Unity Hub and press Play on `Assets/Scenes/Level1`.

## Controls
- Move: A / D or Left / Right
- Jump: W or Up
- Super jump: Space
- Fast fall (in the air): S or Down

## The level
Climb the staircase from the bottom left. The camera follows the chicken.

- **Fire** kills the chicken. It turns into a roasted drumstick and comes back at the last checkpoint.
- **Eggs**: every 15 seconds alive the chicken lays an egg. The latest egg is the checkpoint. If it lands on fire it slides to a safe spot first. The timer restarts after a death.
- **Rocks**: pick up to 3 (counter shown on screen). They are never lost on death.
- **Worms**: only give points when you hold 3 rocks; otherwise you walk through them. Rocks are not used up.

## Scripts (`Assets/`)
PlayerMovement, CameraFollow, Hazard, PlayerLife, EggLayer, Egg, PlayerStats, Collectible.