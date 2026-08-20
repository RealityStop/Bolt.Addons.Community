# 🛠️ Unity Visual Scripting: Community Addons
# Join the discord
[![Discord Banner](https://discord.com/api/guilds/1086044503548366928/widget.png?style=banner2)](https://discord.gg/ny33h6zsQu)

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

![](https://imgur.com/v92tiFD.png)

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
   > We include a multitude of assets, nodes, and tooling to extend missing language concepts and provide fundamental utilities and extensions for graphs. Here is what you will find:

### 🧩 Node Library

| Category | Units | Docs |
| :--- | :--- | :---: |
| **Control Flow** | Branch (Params), Gate, Edge Trigger, Change Detect, Chance Flow, Limited Trigger, Latch, If (Next), ElseIf, Flow/Value Reroute, Flow To Coroutine | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#control) |
| **Events & Delegates** | On Every X Seconds, On Variable Changed, Defined/Return Events, Channel Events, Delegate Binding (Invoke, Bind, Unbind) | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#events) |
| **Variables & Data** | Increment/Decrement, Plus Equals, Save/Clear Variables, Machine Variables, Window Variables | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#variables) |
| **Math & Logic** | Math Op, Negate, Curves (Decay, Sigmoid, Exponential, Linear), Logic Params, Toggle Bool, Polarity, Between | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#logic) |
| **Strings & Text** | StringBuilder, Formatting (Bold, Italic, Underline, Strikethrough), String Manipulation (Reverse, Size, Color, Random String) | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#utility) |
| **Memory & Utilities** | Object Pooling (Init, Retrieve, Return), Clipboard Copy, Counter, Log Node, Stopwatch, Convert, As | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#pooling) |
| **Collections** | Array Operations (Create, Set, Get), Query Node, Random Element, Random Numbers | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#collections) |
| **Documentation** | Comment, Arrow, Todo, Placeholder Nodes(Stuff Happens, Some Value) | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Units-Reference#documentation) |

---

### 💎 Assets & Tools

| Category | Feature | Description | Docs |
| :--- | :--- | :--- | :---: |
| **Code Generation** | C# Asset Generator | Compiles graphs directly into dependency-free C# classes, delegates, enums, interfaces, and structs *(Experimental)*. | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/C%23-Generation) |
| **Editor UI** | Editor Window View | Build custom Unity editor windows directly using Visual Scripting graphs. | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Editor-Windows) |
| **Wizards** | Node Creation Wizard | Automated editor script to generate boilerplate code for new custom units. | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Node-Creation-Wizard) |
| **Wizards** | Descriptor Creation Wizard | Automated editor script to generate unit descriptors. | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Descriptor-Creation-Wizard) |

---

### 🛠️ Workflow Enhancements

| Tool | Function | Docs |
| :--- | :--- | :---: |
| **Selection to Macro** | Instantly collapse selected node clusters into Subgraphs or Macros. | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Selection-to-Macro) |
| **Graph Snippets** | Store, share, and reuse standard node patterns across projects. | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Graph-Snippets) |
| **Node Finder** | Project-wide search tool for finding nodes and other elements within any graph or subgraph. | [Wiki](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Node-Finder) |

---

## 👥 Community & Credits

### Maintainers
* **Active Maintainers:** S2NX7
* **Legacy Authors:** Reality.Stop() • JasonJonesLASM

> [!NOTE]
> **Special Thanks to Contributors:**  
> Necka • AFoolsDuty • Eka • Silence • PurerLogic • Tomate Salat • Spyboticer • omega-ult
