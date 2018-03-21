
# Bolt.Addons.Community
A community-driven project for extending Unity Bolt with custom nodes, types, and helpers.

Development is open to the community.


#### Why Bolt.Addons.Community and not Bolt.Community.Addons?  
While the latter flows off of the tongue more naturally, this project aims to be part of a larger ecosystem of addons and so aims for the former.  If auto-adding custom types from namespaces becomes a supported  feature of Bolt, Bolt.Addons.X is preferable.

## Direct Download (requires matching Bolt version)
|Bolt|  |
|--|--|
| 1.3 | [Bolt.Addons.Community.Variables](https://www.dropbox.com/s/a7rn56n7fva0jv4/Bolt.Addons.Community.Variables.dll?dl=0) |
| 1.3 | [Bolt.Addons.Community.Math](https://www.dropbox.com/s/9vqwczyea3i7nuf/Bolt.Addons.Community.Math.dll?dl=0) |
| 1.3 | [Bolt.Addons.Community.Logic](https://www.dropbox.com/s/xnxx24795t1cj3s/Bolt.Addons.Community.Logic.dll?dl=0) |

## Installing
Add the dlls via the Unit Option Wizard in the assembly step and regenerate.  There is no need to add anything to the type options.


## What's included
**Bold** items are new units included in the dlls.  More coming soon!

### Bolt.Addons.Community.Variables

 - **Increment Variable**
 - **Decrement Variable**

### Bolt.Addons.Community.Math

 - Functions
	 - **Decay**
	 - **Exponential**
	 - **Linear**
	 - **Logarithmic**
	 - **Reverse Linear**
	 - **Sigmoid**

### Bolt.Addons.Community.Logic

 - **Branch Next**


----------

# For Developers:

## How to build
Add the following dlls from your Unity Project with Bolt installed:

\Dependencies\BoltBinaries
 - Bolt.Core.Runtime.dll
 - Bolt.Flow.Runtime.dll
 - Bolt.State.Runtime.dll
 - Ludiq.Core.Runtime.dll
 - Ludiq.Graphs.Runtime.dll
  
\Dependencies\UnityBinaries
 - UnityEditor.dll
 - UnityEngine.CoreModule.dll
 - UnityEngine.dll
 - UnityEngine.UI.dll

Open and build the \src\Bolt.Addons.Community.sln solution in Visual Studio 2017.


## Current contributors (aka who to blame)
 - Reality.Stop()
 - JasonJamesLASM
