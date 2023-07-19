> This project is no longer maintained.  The orginal creators have moved on and are no longer active in the Bolt community.  
>
> However, this is an open source project!  Check for forks, create one yourself, or become an active maintainer!





# Community Addons
### Unity Visual Scripting Extensions
A community-driven project for extending Unity Visual Scripting with custom nodes, assets, types, and helpers.

Development is open to the community.

----------

# Installing

To import the addon, use the following method:


> **Important**: if updating from a pre-3.0 version, please *DELETE* any Bolt.Addons.Community dll files.  By default these were placed in your *Plugins* folder.

### Via Package Manager:

Open the Unity Package Manager, and click the "+" button in the top-left corner :

![](https://imgur.com/v92tiFD.png)

and add the following url:

> https://github.com/S2NX7/Bolt.Addons.Community.git

(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)

Then, use the Regenerate Nodes option in Project Settings > Visual Scripting, and they're ready to go!  Once you've rebuilt your node options, the new nodes will be available for use.
	
Then, use the Regenerate Nodes option in Project Settings > Visual Scripting, and they're ready to go!  Once you've rebuilt your node options, the new nodes will be available for use.


# Updating
To update, open Packages/manifest.json and remove the dev.bolt.addons entry under lock at the end of the file.  Once Unity synchronizes the package source, use the Tools menu to Build Unit Options and incorporate any new units.

![](https://imgur.com/siRm7wu.gif)

----------


### What's included
We include a multitude of assets, nodes, and tooling to extend missing language concepts and provide fundamental utilities and extensions for graphs. Here is what you will find:


--------


> ### NODES

Variables
 - **Increment Variable**
 - **Decrement Variable**
 - **Plus Equals**

Events
 - **On Every X Seconds**
 - **On Variable Changed**
 - **Manual Event**
 - **Defined Event**
 - **Trigger Defined Event**
 - **Global Defined Event**
 - **Trigger Global Defined Event**
 - **Return Event**
 - **Event Return**
 - **Trigger Return Event**
 - **Editor Window Events**
 - **Trigger Asset Custom Event**
 - **Reset Graph Listener**
 - **ChannelEvent**
 - **TriggerChannelEvent**
 - **Wait for Task Event**
 - **Send Task Event**

Documentation
 - **Todo**
 - **Some Value**
 - **Stuff Happens**
 - **Comment**

Collections
 - **Random Numbers**
 - **Create Array**
 - **Get Array Item**
 - **Set Array Item**
 - **Query**
 - **Random Element**

Control
 - **Branch (Params)**
 - **Gate**
 - **Edge Trigger**
 - **Change Detect**
 - **Invoke Delegate**
 - **Bind Delegate**
 - **Unbind Delegate**
 - **Create Delegate**
 - **Limited Trigger**
 - **Chance Flow**
 - **FlowToCoroutine**
 - **CoroutineToFlow**

Logic
 - **Latch**
 - **Polarity**
 - **Between**
 - **Logic (Params)**
 - **Log**
 - **Toggle Bool**

Utility
 - **Convert**	
 - **Flow Reroute**
 - **Value Reroute**
 - **Copy To Clipboard**
 - **Counter**
 - **Is Empty or Whitespace**
   
String
 - **Bold**
 - **Size**
 - **Color**
 - **Italiz**
 - **Strikethrough**
 - **Underline**
 - **Reverse String**

Object Pooling
 - **Initialize Object Pool**
 - **Retrive Object From Pool**
 - **Return Object To Pool**
 - **Return All Objects To Pool**

Graphs
 - **Machine Is**
 - **Get Machine**
 - **Get Machines**
 - **Get Machine Variable**
 - **Set Machine Variable**
 - **Has Machine Variable**
 
Time
 - **Yield**
 - **Enumerator**

Editor
 - **Window Is**
 - **Get Window Variable**
 - **Set Window Variable**

Math
 - **Math Op**
 - **Negate Value**
 - **Functions**
	 - **Decay**
	 - **Exponential**
	 - **Linear**
	 - **Logarithmic**
	 - **Reverse Linear**
	 - **Sigmoid**


--------

> ### ASSETS

Code Assets are used to generate dependency free C# code.

Code
 - **Class** (Experimental)
 - **Delegate**
 - **Enum**
 - **Interface** (Experimental)
 - **Struct** (Experimental)

Editor
 - **Editor Window**
 - **Unit Generator Window**
 - **Unit Descriptor Generator Window**
   
   (The Unit Generator doesnt generate logic for the
   Unit only the Ports this generator is just a time saver, You have to add your own logic)


--------


> ### UTILITIES

 - **Node Selection to Asset or Embed**
 - **Compiler Button**


--------


## Current maintainers (aka who to blame)
 - Reality.Stop()
 - JasonJonesLASM
 
 With contributions from:
 - Necka
 - AFoolsDuty
 - Eka
 - Silence
 - PurerLogic
 - S2NX7
