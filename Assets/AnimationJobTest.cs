using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

// About this issue:
// 
// `ProcessRootMotion` and `ProcessAnimation` methods of the AnimationJob will not be executed if the `AnimationScriptPlayable` is 
// output to a `ScriptPlayable` and it’s **not the first(at index 0)** input of the `ScriptPlayable`.
// 
// How to reproduce:
// 
// 1. Open the "SampleScene".
// 2. Enter play mode.
// 3. In the Game view, you will see "Connect to Port 0" and the value of the counter is increasing.
// 4. Click the "Switch Connection" button, and you will see "Connect to Port 1" and the value of the counter has **unexpectedly** stopped increasing.
// 
// View the topology of the PlayableGraph in the PlayableGraph Monitor (Tools/Bamboo/PlayableGraph Monitor). 
// 
// Solution：
// 
// You can temporarily fix this issue by inserting an `AnimationLayerMixerPlayable` between the `ScriptPlayable` and the `AnimationScriptPlayable`.
// Please refer to AnimationJobTest_TempFix.cs for more details.

public struct MyAnimJob : IAnimationJob
{
    public ulong motionCounter;
    public ulong poseCounter;
    public void ProcessRootMotion(AnimationStream stream) => motionCounter++;
    public void ProcessAnimation(AnimationStream stream) => poseCounter++;
}

public class MyPlayableBehaviour : PlayableBehaviour
{
}

[RequireComponent(typeof(Animator))]
public class AnimationJobTest : MonoBehaviour
{
    public Text connectionText;
    public Text motionText;
    public Text poseText;
    public Button switchButton;

    private PlayableGraph _graph;
    private ScriptPlayable<MyPlayableBehaviour> _sp;
    private AnimationScriptPlayable _asp;
    private bool _connectAspToFirstPort = true;

    private void Awake()
    {
        switchButton.onClick.AddListener(SwitchConnection);

        _graph = PlayableGraph.Create("Animation Job Test");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        _sp = ScriptPlayable<MyPlayableBehaviour>.Create(_graph, 2);
        _asp = AnimationScriptPlayable.Create(_graph, new MyAnimJob());

        connectionText.text = _connectAspToFirstPort ? "Connect to Port 0" : "Connect to Port 1";
        if (_connectAspToFirstPort) _sp.ConnectInput(0, _asp, 0, 1f);
        else _sp.ConnectInput(1, _asp, 0, 1f);

        var animOutput = AnimationPlayableOutput.Create(_graph, "Anim Output", GetComponent<Animator>());
        animOutput.SetSourcePlayable(_sp);

        _graph.Play();
    }

    private void LateUpdate()
    {
        var jobData = _asp.GetJobData<MyAnimJob>();
        motionText.text = $"ProcessRootMotion Counter: {jobData.motionCounter}";
        poseText.text = $"ProcessAnimation Counter: {jobData.poseCounter}";
    }

    private void OnDestroy()
    {
        _graph.Destroy();
    }

    public void SwitchConnection()
    {
        _connectAspToFirstPort = !_connectAspToFirstPort;
        connectionText.text = _connectAspToFirstPort ? "Connect to Port 0" : "Connect to Port 1";

        _sp.DisconnectInput(0);
        _sp.DisconnectInput(1);
        if (_connectAspToFirstPort) _sp.ConnectInput(0, _asp, 0, 1f);
        else _sp.ConnectInput(1, _asp, 0, 1f);
    }
}