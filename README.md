## Game Overview

A small slot machine game built with Unity. Choose a bet, spin the reels, and try to land the same symbol on every reel. A win pays 10 times the bet; a non-matching result loses the wager. The game starts with $5,000.

## Instructions to Run WebGL Build

### Build with Unity

1. Open this project in Unity Hub using Unity 6000.3.6f1.
2. Open File > Build Profiles and select WebGL. Install the WebGL Build Support module for this Unity version if prompted.
3. Select Switch Platform if WebGL is not the active platform.
4. Confirm `Assets/Scenes/SampleScene.unity` is included in the build scenes, then choose Build or Build And Run and select an

## Thought Process or Approach

The game separates reel behavior from the interaction and betting flow. The reel controller generates a result for each reel, animates the strips, and checks whether all results match. The UI handler collects a bet, updates the balance, starts the spin, and displays the outcome. This keeps the win rule straightforward and makes UI feedback and wager handling easy to follow independently.
