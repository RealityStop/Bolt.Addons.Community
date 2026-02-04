# 🛠️ Unity Visual Scripting: Community Addons
# Join the discord
[![Discord Banner](https://discord.com/api/guilds/1086044503548366928/widget.png?style=banner2)](https://discord.gg/TdqJhZG3Nd)

A community-driven project extending Unity Visual Scripting with custom nodes, assets, helpers and more! 

> [!IMPORTANT]
> The original creators have moved on. This project is now managed by new maintainers!

---

## 🌿 Branches & Compatibility
*   **`master`**: Targets the current **Unity Visual Scripting**.
*   **`bolt-main`**: Legacy support for **Bolt**. *(No longer receiving updates)*.

---

## 🚀 Installation

### Option 1: Via Package Manager (Recommended)
1. Open the Unity Package Manager.
2. Click the **+** button > **Add package from git URL...**
3. Paste the following:
   - **For Visual Scripting:** `https://github.com/RealityStop/Bolt.Addons.Community.git`
   - **For Bolt:** `https://github.com/RealityStop/Bolt.Addons.Community.git#bolt-main`

### Option 2: Manual Installation
Add the following line to your `Packages/manifest.json` under `dependencies`:

```json
"dev.bolt.addons": "https://github.com/RealityStop/Bolt.Addons.Community.git"
```

> [!WARNING]
> If updating from **pre-3.0**, please **DELETE** any `Bolt.Addons.Community.dll` files in your `Plugins` folder before installing.

**Final Step:** Go to `Project Settings > Visual Scripting` and click **Regenerate Nodes**.

---

## 📦 What's Included?
We include a multitude of assets, nodes, and tooling to extend missing language concepts and provide fundamental utilities and extensions for graphs. Here is what you will find:

### 📦 Node Library

| Category | |
| :--- | :--- |
| [**Variables**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#variables) | Increment, Decrement, Plus Equals, Reset Saved, Clear Saved |
| [**Events**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#events) | On Every X Seconds, On Variable Changed, Manual Event, Defined Event, Trigger Defined Event, Global Defined Event, Return Event, Event Return, Trigger Return Event, Editor Window Events, Trigger Asset Custom Event, Reset Graph Listener, Channel Event, Trigger Channel Event |
| [**Control**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#control) | Branch (Params), Gate, Edge Trigger, Change Detect, Invoke Delegate, Bind Delegate, Unbind Delegate, Create Delegate, FlowToCoroutine, CoroutineToFlow, WaitForManualPress, If (Next), ElseIf, Using, Flow Reroute, Value Reroute, Chance Flow, Limited Trigger |
| [**Collections**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#control) | Create Array, Get Array Item, Query Node, Random Element, Random Numbers, Set Array Item |
| [**Logic**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#logic) | Logic Params, Log Node, Toggle Bool, Gate, Polarity, Between, Edge Trigger, Latch |
| [**Utility**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#logic) | Convert, As, Copy To Clipboard, Counter, IsStringEmptyOrWhitespace, HDRColors, Select Expose, Bold, Italic, Strikethrough, Underline, Reverse String, String Builder, Size, Color, Random String, Multiline String |
| [**Documentation**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#documentation) | Comment, Arrow, Todo, Stuff Happens, Some Value |
| [**Object Pooling**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#pooling) | Initialize Object Pool, Retrieve Object, On Retrieved, Return Object, Return All Objects, On Returned |
| [**Graphs**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Machine-Variables) | Machine Is, Get Machine, Get Machine Variable, Get Machines, Set Machine Variable, Has Machine Variable |
| [**Time**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Machine-Variables) | Yield, Enumerator, Stopwatch |
| [**Editor Window View**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Editor-Window-View) | Window Is, Get Window Variable, Set Window Variable |
| [**Math**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Editor-Window-View) | Math Op, Negate Value, Decay, Exponential, Linear, Logarithmic, Reverse Linear, Sigmoid |

### 💎 Assets
*Code Assets generate dependency-free C# code directly from your graphs.*

| Category | Type | Description |
| :--- | :--- | :--- |
| [**Code**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/C%23-Generation) | Class, Delegates, Enums, Interface, Struct | Essential Visual Scripting to C# building blocks. (Experimental) |
| [**Editor**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Editor-Windows) | Window View) | Tool for custom UI. |
| **Wizards** | [Node](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Node-Creation-Wizard) & [Descriptor Creation](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Descriptor-Creation-Wizard) | Tools for faster unit and descriptor creation. |

---

### 🛠️ Utilities
*Workflow enhancements to speed up your development process.*

| Utility Tool | Function |
| :--- | :--- |
| [**Selection to Macro**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Selection-to-Macro) | Quickly convert a group of nodes into a Subgraph (Embed or Macro). |
| [**Graph Snippets**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Graph-Snippets) | Save and inject common node patterns (Control & Value). |
| [**Node Finder**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Node-Finder) | Global search utility for finding nodes and other elements within any graph or subgraph. |

---

## 👥 Community Credits

### 🛡️ Maintainers
*The current and past architects of the project.*

| Current | Legacy (The Original Team) |
| :--- | :--- |
| **S2NX7** | Reality.Stop() • JasonJonesLASM |

> [!NOTE]
> ### 🌟 Contributors
> *A special thanks to those who have helped shape this project.*
> 
> Necka • AFoolsDuty • Eka • Silence • PurerLogic • Tomate Salat • Spyboticer • omega-ult
