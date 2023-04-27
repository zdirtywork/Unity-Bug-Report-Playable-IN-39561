# Unity-Bug-Report-Playable-IN-39561
If the `AnimationScriptPlayable` is output to a `ScriptPlayable` and it is not the **first** input of the `ScriptPlayable`, the `ProcessRootMotion` and `ProcessAnimation` methods of the AnimationJob will not be executed.
