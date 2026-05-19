# Coin Pusher Arcade Game

🎮 A realistic 3D mobile Coin Pusher Arcade Game for Android and iOS, inspired by classic casino and carnival coin pusher machines.

## Features

### Core Gameplay
- **Realistic Physics**: Satisfying coin physics with stacking, sliding, bouncing, and falling
- **Interactive Pusher**: Tap and hold to drop coins onto the moving pusher platform
- **Reward System**: Coins, gems, tickets, mystery boxes, and progressive jackpots
- **Combo Multipliers**: Chain reactions when many coins fall simultaneously
- **Special Coins**: Glowing coins with bonus effects and rare drops

### Multiple Themed Machines
- 🏴‍☠️ Pirate Treasure
- 🤖 Cyber Neon
- ⛏️ Gold Rush
- 🍬 Candy World
- 🏛️ Ancient Egypt

### Progression Systems
- Daily rewards and login bonuses
- Missions and achievements
- Level progression with XP system
- Unlockable coin skins and machine designs
- Lucky wheel spinner
- Offline earnings system

### In-Game Shop & Upgrades
- Coin drop speed boosts
- Coin value multipliers
- Push power enhancements
- Auto-drop feature
- VIP membership system
- Seasonal battle pass rewards

### Monetization
- AdMob integration (Rewarded & Interstitial ads)
- In-app purchases (Coin/Gem packs, Premium themes, Ad removal)
- Battle pass seasonal rewards
- Optimized for high retention and passive income

### Social & Competitive
- PvP leaderboard rankings
- Clan/guild system
- Google Play Games & Apple Game Center support
- Seasonal events with limited skins

### Advanced Systems
- Firebase Analytics integration
- Cloud save system
- Rare coin drops
- Random treasure events
- Timed bonus rounds
- Golden coin rain events
- Spin-to-win bonus machine

## Technical Specifications

- **Engine**: Unity (C#)
- **Target Platforms**: Android & iOS
- **Optimization**: Mobile-optimized for low-end devices
- **Physics**: PhysX engine with custom coin simulation
- **Save System**: PlayerPrefs + Cloud saves
- **Analytics**: Firebase Analytics
- **Ads**: Google AdMob
- **Social**: Google Play Games + Apple Game Center

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/                    # Core game systems
│   ├── Physics/                 # Coin physics engine
│   ├── UI/                      # UI controllers and managers
│   ├── Gameplay/                # Game mechanics
│   ├── Monetization/            # AdMob and IAP systems
│   ├── Analytics/               # Firebase and tracking
│   ├── Save/                    # Save/load systems
│   └── Utils/                   # Utility functions
├── Scenes/
│   ├── Splash.unity
│   ├── MainMenu.unity
│   ├── Machines/
│   │   ├── PirateTreasure.unity
│   │   ├── CyberNeon.unity
│   │   ├── GoldRush.unity
│   │   ├── CandyWorld.unity
│   │   └── AncientEgypt.unity
│   └── Shop.unity
├── Prefabs/
│   ├── Coins/
│   ├── UI/
│   └── VFX/
├── Materials/
├── Textures/
├── Audio/
│   ├── SFX/
│   └── Music/
└── Resources/
```

## Getting Started

### Prerequisites
- Unity 2022 LTS or newer
- Visual Studio or Rider
- Android SDK (for Android build)
- Xcode (for iOS build)

### Installation

1. Clone the repository
```bash
git clone https://github.com/richardsonlumar-png/Coin-Pusher-Arcade-Game.git
```

2. Open in Unity Hub
3. Install required packages (see `ProjectSettings/ProjectVersion.txt`)
4. Configure AdMob and Firebase (see SETUP_GUIDE.md)

### Building

**Android**
```bash
Unity -batchmode -projectPath . -executeMethod BuildHelper.BuildAndroid -quit
```

**iOS**
```bash
Unity -batchmode -projectPath . -executeMethod BuildHelper.BuildiOS -quit
```

## Configuration

### AdMob Setup
See `Documentation/ADMOB_SETUP.md` for complete instructions.

### Firebase Setup
See `Documentation/FIREBASE_SETUP.md` for configuration guide.

### Mobile Optimization
See `Documentation/OPTIMIZATION.md` for performance tuning.

## Performance Targets

- Target FPS: 60 FPS on mid-range devices (2-3 year old phones)
- Memory Usage: < 500MB
- APK Size: < 150MB
- Startup Time: < 3 seconds

## Monetization Strategy

### Revenue Streams
1. **Rewarded Ads**: 35% of revenue
   - Extra coins after session
   - Lucky wheel spins
   - Skip timers

2. **Interstitial Ads**: 25% of revenue
   - Between sessions
   - Machine transitions

3. **In-App Purchases**: 30% of revenue
   - Coin packs ($0.99 - $99.99)
   - Gem packs
   - Premium themes
   - Ad removal

4. **Battle Pass**: 10% of revenue
   - Seasonal premium track
   - Exclusive rewards

## Key Mechanics

### Coin Physics
- Realistic gravity simulation
- Coin-to-coin collision
- Stacking behavior
- Friction and bounce parameters
- Dynamic velocity calculations

### Reward System
- Base coin value: 1-100
- Gem multiplier: 2-50x
- Ticket conversion: 100 coins = 1 ticket
- Mystery box: Random 50-500 coins
- Jackpot: Progressive meter

### Combo System
- x2 multiplier at 10+ simultaneous coins
- x5 multiplier at 25+ coins
- x10 multiplier at 50+ coins
- Chain reaction bonus: 1.2x per consecutive win

## Development Roadmap

### Phase 1 (Week 1-2)
- Core physics system
- Basic UI
- First machine theme (Pirate)

### Phase 2 (Week 3-4)
- Additional themes
- Progression system
- In-game shop

### Phase 3 (Week 5-6)
- AdMob & IAP integration
- Firebase analytics
- Social features

### Phase 4 (Week 7-8)
- Optimization & testing
- Build for Android/iOS
- App store submission

## Contributing

This is a solo project. For contributions or collaborations, please contact the developer.

## License

All rights reserved. © 2026

## Support

For issues, questions, or feedback, please create an issue in the GitHub repository.

---

**Made with ❤️ in Unity**
