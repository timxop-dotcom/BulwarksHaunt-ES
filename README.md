# Bulwark's Haunt
Adds an alternate ending.  
To reach it, find the Crystalline Blade on Sky Meadow and bring it to the Obelisk.  
Note: the ending is not quick and *will* take some time to complete!  
Created in 4 days by TheMysticSword (TheMysticSword#9770 on Discord) for the Halloween Modjam 2022, entirely without the use of Thunderkit as an experiment.  
  
![Bulwark's Grave](https://i.imgur.com/dwe6CVo.png)  
  
![Stage Preview](https://i.imgur.com/OpSdxNY.png)

# Known Issues
* Main map terrain has odd triangular shading
* Not ProperSave-compatible
  
# Credits
Sword model by marwan60
## External assets
Using [Graveyard Kit](https://opengameart.org/content/graveyard-kit) by [Kenney.nl](https://kenney.nl/), licensed under CC0 1.0 Universal.  
Contains assets from [Poly Haven](https://polyhaven.com/), licensed under CC0 1.0 Universal.  
Contains assets from [ambientCG.com](https://ambientCG.com/), licensed under CC0 1.0 Universal.

# Changelog
## 1.0.5:
* Dead players are now respawned after each wave
* Changed the colour of the ghosts to green to make them distinguishable from Happiest Mask ghosts
* Reduced initial monster spawn director credits
* Reduced the credit gain for the elite wisp spawn director
* Fixed Russian language file encoding issue which caused all text from this mod to show up as question marks
* Fixed the wave break timer not showing up for non-host players
* Fixed the wave counter not increasing for non-host players
## 1.0.4:
* Fixed the stage replacing the destinations of all Lunar Seers after stage 8
* Fixed obscene EXP and gold drops from non-ghost enemies
* Fixed non-ghost enemy directors spawning enemies too infrequently
* Fixed ghost corpses not despawning if Corpse Clean-up was set to Hidden
* Added a hard enemy cap per wave to prevent waves that last too long
* The fallback enemy spawner is now closer to real gameplay, starting off with easier enemies in the first waves, and harder and more numerous enemies in the later waves
## 1.0.3:
* Fixed an issue with attempting to spawn enemies that lack a CharacterMaster, should prevent a softlock with modded enemies
## 1.0.2:
* Nodegraphs are now prebaked, significantly improving the load time
* Added an extra out of bounds enemy killer that should be consistent
* Tweaked wisp experience and money drops
* Tweaked wisp spawns
* Actions in Hidden Realms are no longer tracked for the event
* Improved dead/failed-to-spawn enemy detection to avoid softlocks
* Removed ghost health decay
* Moved the out of bounds ceiling zone lower
* Fixed error spam with MoonstormSharedUtils
* Fixed missing SurfaceDefs on trees
## 1.0.1:
* Updated README with screenshots and extra info
* Added extra fallback behaviour in case of getting to the stage too early
* Fixed a bug with enemies being spawned too quickly
