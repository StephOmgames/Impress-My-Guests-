# Impress My Guests

A multiplayer home-design game built in **Unity 2022.3 LTS**.  
Create your character, design a stunning home, invite other players as guests, and see whose taste impresses everyone most!

---

## 🏗️ Project Structure

```
Assets/
  Scripts/
    CharacterCreation/   – Character data, creator logic, and UI
    HomeDesign/          – Furniture catalog, room grid, design manager, and UI
    Multiplayer/         – Netcode-based network manager, player sync, home sharing
    Core/                – GameManager singleton and SceneLoader utility
  ScriptableObjects/
    Characters/          – AppearanceOption assets
    Furniture/           – FurnitureCatalog assets
  Scenes/                – MainMenu, CharacterCreation, HomeDesign, MultiplayerLobby
ProjectSettings/         – Unity project settings (Unity 2022.3.20f1)
Packages/                – Package manifest (Netcode for GameObjects, TextMeshPro, etc.)
```

---

## 🎮 Gameplay Overview

| Phase | Description |
|-------|-------------|
| **Character Creation** | Choose name, body type, skin tone, hair, eyes, outfit, and personality trait. |
| **Home Design** | Place furniture on a grid-based room layout within a budget. Multiple room types (living room, kitchen, bedroom…). |
| **Multiplayer Visit** | Host or join a session. Browse other players' homes. Rate them to determine who impresses the guests most! |

---

## 🚀 Getting Started

1. Open the project in **Unity 2022.3.20f1** (or later LTS).
2. Let the Package Manager resolve dependencies (`Window → Package Manager`).
3. Open `Assets/Scenes/MainMenu.unity` and press **Play**.

---

## 📦 Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `com.unity.netcode.gameobjects` | 1.8.1 | Multiplayer networking |
| `com.unity.textmeshpro` | 3.0.6 | Rich-text UI labels |

---

## 🗂️ Core Script Reference

### Character Creation
| Script | Role |
|--------|------|
| `CharacterData.cs` | Serialisable data bag (name, appearance, personality) |
| `AppearanceOption.cs` | ScriptableObject for individual appearance choices |
| `CharacterCreator.cs` | Singleton logic layer — mutates/validates `CharacterData` |
| `CharacterCreationUI.cs` | Unity UI bindings for the character-creation screen |

### Home Design
| Script | Role |
|--------|------|
| `FurnitureItem.cs` | Per-prefab component describing a furniture piece |
| `FurnitureCatalog.cs` | ScriptableObject catalog of all furniture prefabs |
| `RoomManager.cs` | Grid-based placement logic for a single room |
| `HomeDesignManager.cs` | Session-level manager: budget, rooms, save snapshots |
| `HomeDesignUI.cs` | Unity UI bindings for the home-design screen |

### Multiplayer
| Script | Role |
|--------|------|
| `GameNetworkManager.cs` | Wraps `NetworkManager`; exposes host/client/server start |
| `PlayerNetworkController.cs` | `NetworkBehaviour` on each player avatar; syncs name tag |
| `HomeShareManager.cs` | RPC-based system for uploading/downloading home snapshots |

### Core
| Script | Role |
|--------|------|
| `GameManager.cs` | App-wide singleton; owns game-state machine and scene routing |
| `SceneLoader.cs` | Static helper for scene transitions with events |

---

## 📝 License

See [LICENSE](LICENSE).