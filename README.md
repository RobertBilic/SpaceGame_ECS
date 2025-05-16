# 🛸 Inverse Tower Defense (Unity DOTS)

**A passion project and technical showcase built using Unity ECS (DOTS).**  
This project flips the traditional tower defense formula and demonstrates advanced data-oriented design using Unity's latest DOTS stack.

---

## 🎮 Game Overview

In **Inverse Tower Defense**, you command a large capital ship steadily advancing through waves of enemy defenses. Rather than placing towers to stop enemies, you're the one moving through a hostile gauntlet, outfitting your ship with turrets and forward weapons as you progress.

- 🛠️ Build and upgrade your capital ship with modular turrets and forward-facing weapons  
- 🤖 Deploy flow-field-controlled fighter units that swarm and support your advance  
- ⏱️ Manipulate time dynamically (1x, 2x, 3x, pause) to control pacing  
- ⚔️ Encounter increasingly complex enemy patterns with reactive AI  

---

## 🧠 Technical Highlights

This project is built entirely with Unity’s Data-Oriented Tech Stack:

### ✅ ECS (Entities 1.3+)
- Pure ECS architecture for all gameplay logic
- System-based separation of concerns (movement, combat, AI, building)

### ✅ Burst & Jobs
- Burst-compiled systems and parallel jobs for performance
- Custom flow-field steering logic for 100+ ships in combat

### ✅ Spatial Optimization
- Custom spatial database and grid partitioning
- Efficient targeting, collision detection, and range queries

### ✅ Modular Build System
- Weapon slot baking with types and IDs
- Serializable ship build data across levels
- Runtime weapon spawning and upgrade requests

---

## 🚀 Features (WIP / Planned)
- [x] Modular weapon placement system  
- [x] Flow-field movement for ships  
- [x] Time manipulation input via Input System
- [x] Basic Movement
- [x] Basic bullet collision
- [ ] Level Generation  
- [ ] Ship Variety
- [ ] UI for building & upgrading weapons  
- [ ] Level progression & enemy escalation  
- [ ] Save/load ship state between levels  

---

## 📷 Screenshots
> _Coming soon: gameplay footage and visual breakdowns_

---

## 🧪 Setup & Requirements

- **Unity 2023 LTS or later**
- **Entities package v1.3.14+**
- **Burst, Collections, and Mathematics packages**

---

## 📂 Folder Structure



This project follows a **feature-based folder layout**, with each gameplay feature organized by:

- `Components/`: All ECS `IComponentData`, `IBufferElementData`, and tags related to the feature
- `System/`: Systems (`ISystem`, `SystemBase`, etc.) handling logic for that feature
- `Authoring/`: MonoBehaviours + Bakers for converting GameObjects into Entities


## 💬 Contact

This project is part of my portfolio.  
Feel free to reach out if you're interested in collaboration, hiring, or feedback.

> 👤 **Author**: Robert Bilic  
> 📧 **Email**: robertb97b@gmail.com

---

## 📝 License

MIT License or specify your preferred license.
