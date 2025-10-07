> The orginal creators have moved on and are no longer active in the Bolt community.
> 
> However, the project is now in the hands of new maintainers!

# Community Addons
### Unity Visual Scripting Extensions
A community-driven project for extending Unity Visual Scripting with custom nodes, assets, types, and helpers.

Development is open to the community.

## Branches

Unity Visual Scripting is now this master branch. To use Bolt, use the branch `bolt-main`. Be aware, the Bolt branch will no longer be receiving updates.

----------

# Installing

To import the addon, use one of the following methods:


> **Important**: if updating from a pre-3.0 version, please *DELETE* any Bolt.Addons.Community dll files.  By default these were placed in your *Plugins* folder.

### Via Package Manager:

Open the Unity Package Manager, and click the "+" button in the top-left corner :

![](https://imgur.com/v92tiFD.png)

and add the following url:

> https://github.com/RealityStop/Bolt.Addons.Community.git

For BOLT use branch `bolt-main` with this url:

> https://github.com/RealityStop/Bolt.Addons.Community.git#bolt-main

(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)

Then, use the Regenerate Nodes option in Project Settings > Visual Scripting, and they're ready to go!  Once you've rebuilt your node options, the new nodes will be available for use.



### Manual install:
Alternatively, open Packages/manifest.json and add this line under dependencies:

	"dev.bolt.addons": "https://github.com/RealityStop/Bolt.Addons.Community.git"
	
(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)
	
Then, use the Regenerate Nodes option in Project Settings > Visual Scripting, and they're ready to go!  Once you've rebuilt your node options, the new nodes will be available for use.


# Updating
To update, open Packages/manifest.json and remove the dev.bolt.addons entry under lock at the end of the file.  Once Unity synchronizes the package source, use the Tools menu to Build Unit Options and incorporate any new units.

![](https://imgur.com/siRm7wu.gif)

----------


### What's included
We include a multitude of assets, nodes, and tooling to extend missing language concepts and provide fundamental utilities and extensions for graphs. Here is what you will find:

--------

> ### NODES
> 
>  Variables
>  - [Increment Variable](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Increment-Variable)
>  - [Decrement Variable](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Decrement-Variable)
>  - [Plus Equals](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---PlusEquals)
>  - [Reset Saved Variables](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ResetSavedVariables)
>  - [Clear Saved Variables](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ClearSavedVariables)
>
>  Events
>  - [On Every X Seconds](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---OnEveryXSecondsNode)
>  - [On Variable Changed](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---OnVariableChangedNode)
>  - [Manual Event](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ManualEvent)
>  - [Defined Event](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---DefinedEventNode)
>  - [Trigger Defined Event](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---TriggerDefinedEvent)
>  - [Global Defined Event](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---GlobalDefinedEventNode)
>  - [Trigger Global Defined Event](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---TriggerGlobalDefinedEvent)
>  - [Return Event](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ReturnEvent)
>  - [Event Return](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---EventReturn)
>  - [Trigger Return Event](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---TriggerReturnEvent)
>  - [Editor Window Events](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---EditorWindowEvents)
>  - [Trigger Asset Custom Event](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---TriggerAssetCustomEvent)
>  - [Reset Graph Listener](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ResetGraphListener)
>  - [ChannelEvent](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ChannelEvent)
>  - [TriggerChannelEvent](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---TriggerChannelEvent)
>
>  Documentation
>   - [Comment](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---CommentNode)  
>   - [Arrow](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Arrow)  
>   - [Todo](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Todo)  
>   - [Stuff Happens](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---StuffHappens)  
>   - [Some Value](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---SomeValue) 
>
>  Collections
>   - [Query Node](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---QueryNode)  
>   - [Random Element](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---RandomElementNode)  
>   - [Random Numbers](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---RandomNumbersv2)   
>   - [Create Array](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---CreateArray)  
>   - [SetArrayItem](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---SetArrayItem)  
>   - [GetArrayItem](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---GetArrayItem)  
>
>  Control
>  - [Branch (Params)](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---BranchParams)
>  - [Gate](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Gate)
>  - [Edge Trigger](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Edge-Trigger)
>  - [Change Detect](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ChangeDetect)
>  - [Invoke Delegate](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ActionInvokeNode)
>  - [Bind Delegate](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---BindActionNode)
>  - [Unbind Delegate](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---UnbindActionNode)
>  - [Create Delegate](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ActionNode)
>  - [Limited Trigger](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---LimitedTriggerNode)
>  - [Chance Flow](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ChanceFlow)
>  - [FlowToCoroutine](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---FlowToCoroutine)
>  - [CoroutineToFlow](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---CoroutineToFlow)
>  - [WaitForManualPress](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---WaitForManualPress)
>  - [If (Next)](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---BetterIf)
>  - [ElseIf](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ElseIfUnit)
>  - [Using](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Using)
>  - [Flow Reroute](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---FlowReroute)
>  - [Value Reroute](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ValueReroute)
>
>  Logic
>   - [Logic Params](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---LogicParams)  
>   - [Log Node](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---LogNode)  
>   - [Toggle Bool](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ToggleBool)  
>   - [Gate](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Gate)  
>   - [Polarity](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Polarity)  
>   - [Between](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Between)  
>   - [Edge Trigger](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Edge-Trigger)  
>   - [Latch](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Latch)
>
>  Utility
>  - [Convert](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ConvertNode)
>  - [As](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---AsUnit)
>  - [Copy To Clipboard](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---CopyToClipboardUnit)
>  - [Counter](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---CounterNode)
>  - [IsStringEmptyOrWhitespace](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---IsStringEmptyOrWhitespace)
>  - [HDRColors](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---HDRColors)
>  - [Select Expose](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---SelectExpose)
>  - [Bold](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---BoldString)
>  - [Italic](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ItalicString)
>  - [Strikethrough](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---StrikethroughString)
>  - [Underline](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---UnderlineString)
>  - [Reverse String](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ReverseStringNode)
>  - [String Builder](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---StringBuilderUnit)
>  - [Size](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---SizeString)
>  - [Color](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ColorString)
>  - [Random String](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---RandomStringNode)
>  - [Multiline String](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---MultilineStringNode)
>
>  Object Pooling
>  - [Initialize Object Pool](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---InitializePoolNode)
>  - [Retrieve Object](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---RetrieveObjectNode)
>  - [Return Object](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ReturnObjectNode)
>  - [Return All Objects](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ReturnAllObjectsToPoolNode)
>  - Events:
>  >    - [On Retrieved](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---OnRetrieved)
>  >    - [On Returned](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---OnReturned)
>           
>  [Graphs](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---Machine-Variables)
>   - **Machine Is**
>   - **Get Machine**
>   - **Get Machines**
>   - **Get Machine Variable**
>   - **Set Machine Variable**
>   - **Has Machine Variable**
>
>  Time
>  - [Yield](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---YieldNode)
>  - [Enumerator](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---EnumeratorNode)
>  - [Stopwatch](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---StopwatchUnit)

>  [Editor Window View](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Editor-Window-View)
>   - **Window Is**
>   - **Get Window Variable**
>   - **Set Window Variable**
>
>  Math
>  - [Math Op](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---MathParamNode)
>  - [Negate Value](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---NegativeValueNode)
>  - Functions:
>   > - [Decay](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---DecayFunction)
>   > - [Exponential](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ExponentialFunction)
>   > - [Linear](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---LinearFunction)
>   > - [Logarithmic](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---LogarithmicFunction)
>   > - [Reverse Linear](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---ReverseLinearFunction)
>   > - [Sigmoid](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Unit-Reference---SigmoidFunction)
> 
> --------
> 
> ### ASSETS
> >
> > Code Assets are used to generate dependency free C# code.
> >
> > Code
> > - [**Class** (Experimental)](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Classes)
> > - [**Delegates**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Delegates)
> > - [**Enums**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Enums)
> > - [**Interface** (Experimental)](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Interfaces)
> > - [**Struct** (Experimental)](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Structs)
> 
> > Editor
> > - [**Editor Window View**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Editor-Window-View)
> > - [**Node Creation Wizard**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Node-Creation-Wizard)
> > - [**Descriptor Creation Wizard**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Descriptor-Creation-Wizard)
> > - [**Graph Snippets**](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Graph-Snippets)
> >	> - **Control Snippet**
> >	> - **Value Snippet**
> 
> --------
> 
> 
> [Utilities](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Utilities)
> 
>  - [Selection to Macro](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Selection-to-Macro)
>  - [Graph Snippets](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Graph-Snippets)
> 
> 
--------

## Current Maintainers
 - S2NX7

## Previous maintainers (aka who to blame)
 - Reality.Stop()
 - JasonJonesLASM
 
 With contributions from:
 - Necka
 - AFoolsDuty
 - Eka
 - Silence
 - PurerLogic
 - Tomate Salat
 - Spyboticer
 - omega-ult
