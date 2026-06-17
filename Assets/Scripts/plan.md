# Unity Visual Novel Migration Plan (Based on Current DialogueScene)

## Current Structure Analysis

Based on the provided screenshots, the project already contains:

```text
DialogueScene
├── Managers
│   └── DialogueManager
├── Canvas
│   ├── Background
│   ├── LeftPortraitRoot
│   ├── RightPortraitRoot
│   ├── CenterPortraitRoot
│   ├── DialoguePanel
│   │   ├── SpeakerText
│   │   ├── DialogueText
│   │   ├── Next
│   │   └── NextIndicator
│   ├── CharacterSetupPanel
│   └── SummaryPanel
```

The existing structure is already suitable for a Visual Novel system and does not need a major UI redesign.

---

# Architecture Goal

Replace the old cutscene approach with:

- ScriptableObject dialogue database
- Multiple characters
- Portrait emotion switching
- Background switching
- Background fade transition
- Chapter intro cutscene
- Memory shard cutscene
- Highlight active speaker
- Dim inactive characters
- Reusable DialogueManager

---

# Scene Strategy

## Recommended

Keep:

```text
MainGameplayScene
```

Add:

```text
Dialogue Canvas Overlay
```

Do NOT create a scene for every cutscene.

Use:

```text
DialogueManager.Play(dialogueData);
```

for:

- Chapter Intro
- Tutorial
- NPC Conversation
- Memory Shard
- Ending

---

# Existing Objects To Reuse

## Background

Use:

```text
Canvas/Background
```

for dynamic background changes.

---

## Portrait Slots

Use:

```text
LeftPortraitRoot
CenterPortraitRoot
RightPortraitRoot
```

No additional portrait roots needed.

---

## Dialogue Panel

Keep:

```text
SpeakerText
DialogueText
Next
NextIndicator
```

---

# ScriptableObject Structure

## CharacterData

```csharp
Character Name
Neutral Portrait
Happy Portrait
Angry Portrait
Shocked Portrait
Confused Portrait
```

Exactly like the screenshot:

```text
MC_Male
├── Neutral
├── Happy
├── Angry
├── Shocked
└── Confused
```

---

## Dialogue Line

```csharp
Speaker
Text
Emotion
Position
Background
```

Example:

```text
Speaker = Rifqi
Emotion = Happy
Position = Left

Text:
"Kita akhirnya berhasil."
```

---

## Dialogue Data

```csharp
List<DialogueLine>
```

Each asset represents:

```text
Chapter1_Intro
MemoryShard_01
MemoryShard_02
Ending_A
```

---

# Portrait System

## Positions

```csharp
Left
Center
Right
```

Mapped to:

```text
LeftPortraitRoot
CenterPortraitRoot
RightPortraitRoot
```

---

# Active Speaker Highlight

When a character speaks:

```text
Speaker Alpha = 1.0
Others Alpha = 0.4
```

Example:

```text
Rifqi speaks

Rifqi = Bright
Mentor = Dim
Hasbul = Dim
```

When Mentor speaks:

```text
Rifqi = Dim
Mentor = Bright
Hasbul = Dim
```

Implementation:

```csharp
CanvasGroup.alpha
```

or

```csharp
Image.color
```

---

# Three Character Support

Supported layout:

```text
[Rifqi] [Mentor] [Hasbul]
```

Mapped:

```text
Left
Center
Right
```

Dialogue line determines which portrait is active.

---

# Emotion System

Enum:

```csharp
Neutral
Happy
Angry
Shocked
Confused
```

DialogueManager automatically selects portrait sprite.

---

# Background System

Use existing:

```text
Canvas/Background
```

Dialogue line can override background.

Example:

```text
Office
MeetingRoom
Lobby
ServerRoom
```

---

# Background Fade

Add:

```text
BackgroundFadeCanvas
```

or

```text
CanvasGroup
```

Flow:

```text
Fade Out
Change Sprite
Fade In
```

Recommended duration:

```text
0.5 seconds
```

---

# Chapter Intro Flow

```text
Scene Loaded
↓
ChapterManager
↓
Play Intro Dialogue
↓
Gameplay Enabled
```

Pseudo:

```csharp
Start()
{
    DialogueManager.Play(chapterIntro);
}
```

---

# Memory Shard Flow

Gameplay:

```text
Place Item
↓
Puzzle Complete
↓
Reward Memory Shard
```

After unlock:

```text
Memory Shard Added
↓
Player Opens Collection
↓
Player Clicks Watch
↓
DialogueManager.Play(memoryDialogue)
```

---

# Memory Shard Data

```csharp
ID
Title
Thumbnail
DialogueData
Unlocked
```

---

# Remove Old Cutscene System

Safe to remove if unused:

```text
Timeline Assets
Playable Directors
Cutscene Controllers
Timeline Triggers
```

Keep only:

```text
DialogueManager
ScriptableObjects
MemoryShardSystem
```

---

# Scripts To Create

```text
VNCharacterData.cs
VNDialogueLine.cs
VNDialogueData.cs
DialogueManager.cs
DialogueTrigger.cs
ChapterManager.cs
MemoryShardData.cs
MemoryShardManager.cs
BackgroundFader.cs
PortraitSlot.cs
```

---

# Implementation Order

Phase 1
- VNCharacterData
- VNDialogueLine
- VNDialogueData

Phase 2
- DialogueManager Refactor

Phase 3
- Portrait Position System

Phase 4
- Active Speaker Highlight

Phase 5
- Emotion Switching

Phase 6
- Background Switching

Phase 7
- Background Fade

Phase 8
- Chapter Intro

Phase 9
- Memory Shard Integration

Phase 10
- Save/Load Progress

---

# Final Gameplay Flow

```text
Game Start
↓
Chapter Intro Dialogue
↓
Gameplay
↓
Player Places Item
↓
Puzzle Solved
↓
Memory Shard Unlocked
↓
Watch Memory
↓
Dialogue Sequence
↓
Return To Gameplay
```
