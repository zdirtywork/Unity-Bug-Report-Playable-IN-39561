using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class AnimationJobTest_TempFix : MonoBehaviour
{
    public Text connectionText;
    public Text motionText;
    public Text poseText;
    public Button switchButton;

    private PlayableGraph _graph;
    private ScriptPlayable<MyPlayableBehaviour> _sp;
    private AnimationLayerMixerPlayable _almp; // Used as a temporary fix for the issue
    private AnimationScriptPlayable _asp;
    private bool _connectAspToFirstPort = true;

    private void Awake()
    {
        switchButton.onClick.AddListener(SwitchConnection);

        _graph = PlayableGraph.Create("Animation Job Test");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        _sp = ScriptPlayable<MyPlayableBehaviour>.Create(_graph, 1);
        _almp = AnimationLayerMixerPlayable.Create(_graph, 2);
        _asp = AnimationScriptPlayable.Create(_graph, new MyAnimJob());

        // Insert a mixer between the _sp and the _asp to temporarily fix the issue
        connectionText.text = _connectAspToFirstPort ? "Connect to Port 0" : "Connect to Port 1";
        _sp.ConnectInput(0, _almp, 0, 1f);
        _almp.SetInputWeight(0, 1f);
        _almp.SetInputWeight(1, 1f);
        if (_connectAspToFirstPort) _almp.ConnectInput(0, _asp, 0, 1f);
        else _almp.ConnectInput(1, _asp, 0, 1f);

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

        _almp.DisconnectInput(0);
        _almp.DisconnectInput(1);
        if (_connectAspToFirstPort) _almp.ConnectInput(0, _asp, 0, 1f);
        else _almp.ConnectInput(1, _asp, 0, 1f);
    }
}