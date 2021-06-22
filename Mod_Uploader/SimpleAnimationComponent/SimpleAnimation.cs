using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;
using Sirenix.OdinInspector;
using System;
using UniRx;

[RequireComponent(typeof(Animator))]
public partial class SimpleAnimation : MonoBehaviour
{
#if UNITY_EDITOR

    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    protected int _StateCount;
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    protected float _TimeRate;
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    protected float _CurTime;
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    protected float _MaxTime;

    private void Update()
    {
        if (gameObject == Selection.activeGameObject)
        {
            _StateCount = m_Playable.StateCount();
            _TimeRate = m_Playable.TimeRate();
            _CurTime = m_Playable.CurTime();
            _MaxTime = m_Playable.MaxTime();
        }
    }


    public State this[string name]
    {
        get { return GetState(name); }
    }

    [ButtonGroup("Buttons"), Button]
    void PlayEnter()
    {
        ppPlay("Enter");
    }
    [ButtonGroup("Buttons"), Button]
    void PlayExit()
    {
        ppPlay("Exit");
    }
    [ButtonGroup("Buttons"), Button]
    void PlayShow()
    {
        ppPlay("Show");
    }
    [ButtonGroup("Buttons"), Button]
    void PlayHide()
    {
        ppPlay("Hide");
    }
    [ButtonGroup("Buttons"), Button]
    void PlayIdle()
    {
        ppPlay("Idle");
    }
    [ButtonGroup("Buttons"), Button]
    void PlayDefaultTest()
    {
        PlayDefault();
    }
    // [ButtonGroup("Buttons"), Button]
    void PlayState(string state)
    {
        ppPlay(state);
    }

#endif


    private void LateUpdate()
    {
        //高速化対応で再生終わったらアニメーターを動的にオンオフする
        //----------------------------------------------------------
        if (!animator.enabled)
        {
            return;
        }

        var index = m_Playable.FirstPlayingIndex();

        if (index == -1 || (isFinish(index) && !isLoop(index)))
        {
            animator.enabled = false;
        }
        //----------------------------------------------------------
    }


    public interface State
    {
        bool enabled { get; set; }
        bool isValid { get; }
        float time { get; set; }
        float normalizedTime { get; set; }
        float speed { get; set; }
        string name { get; set; }
        float weight { get; set; }
        float length { get; }
        AnimationClip clip { get; }
        WrapMode wrapMode { get; set; }

    }

    bool _HasAnimator;
    public Animator animator
    {
        get
        {
            if (!_HasAnimator)
            {
                m_Animator = GetComponent<Animator>();
                _HasAnimator = true;
            }
            return m_Animator;
        }
    }

    public bool animatePhysics
    {
        get { return m_AnimatePhysics; }
        set { m_AnimatePhysics = value; animator.updateMode = m_AnimatePhysics ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal; }
    }

    public AnimatorCullingMode cullingMode
    {
        get { return animator.cullingMode; }
        set { m_CullingMode = value; animator.cullingMode = m_CullingMode; }
    }

    public int StateCount()
    {
        return m_Playable.StateCount();
    }

    /// <summary>再生終了したかどうか</summary>
    public bool isFinish(int i = 0)
    {
        return (m_Playable.MaxTime(i) <= 0 || m_Playable.TimeRate(i) >= 1.0);
    }

    /// <summary>ループ再生かどうか</summary>
    public bool isLoop(int i = 0)
    {
        return m_Playable.IsLoop(i);
    }

    /// <summary>0~1で、1なら終了している</summary>
    public float TimeRate(int i = 0)
    {
        return m_Playable.TimeRate(i);
    }

    /// <summary>現在の再生時間</summary>
    public float CurTime(int i = 0)
    {
        return m_Playable.CurTime(i);
    }

    /// <summary>最大再生時間</summary>
    public float MaxTime(int i = 0)
    {
        return m_Playable.MaxTime(i);
    }


    public bool isPlaying { get { return m_Playable.IsPlaying(); } }

    public bool playAutomatically
    {
        get { return m_PlayAutomatically; }
        set { m_PlayAutomatically = value; }
    }

    public AnimationClip clip
    {
        get { return m_Clip; }
        set
        {
            LegacyClipCheck(value);
            m_Clip = value;
        }
    }

    public WrapMode wrapMode
    {
        get { return m_WrapMode; }
        set { m_WrapMode = value; }
    }

    public void AddClip(AnimationClip clip, string newName)
    {
        LegacyClipCheck(clip);
        AddState(clip, newName);
    }

    public void Blend(string stateName, float targetWeight, float fadeLength)
    {
        animator.enabled = true;
        Kick();
        m_Playable.Blend(stateName, targetWeight, fadeLength);
    }

    public void CrossFade(string stateName, float fadeLength)
    {
        animator.enabled = true;
        Kick();
        m_Playable.Crossfade(stateName, fadeLength);
    }

    public void CrossFadeQueued(string stateName, float fadeLength, QueueMode queueMode)
    {
        animator.enabled = true;
        Kick();
        m_Playable.CrossfadeQueued(stateName, fadeLength, queueMode);
    }

    public int GetClipCount()
    {
        return m_Playable.GetClipCount();
    }

    public bool IsPlaying(string stateName)
    {
        return m_Playable.IsPlaying(stateName);
    }

    public void Stop()
    {
        if (m_Playable != null)//ネット開始時にヌルになるタイミングあるようなのでヌルチェックします @2020/3/36 Yasuomi Sakai
        {
            m_Playable.StopAll();
        }
    }

    public void Stop(string stateName)
    {
        m_Playable.Stop(stateName);
    }

    public void Sample()
    {
        m_Graph.Evaluate();
    }

    public bool Play(bool checkPlayAutomatically = true)
    {
        animator.enabled = true;
        Kick();
        if (m_Clip != null && (!checkPlayAutomatically || (checkPlayAutomatically && m_PlayAutomatically)))//←理由はよくわかりませんがPlayAutomaticallyじゃないとPlayできない状態だったので引数でチェックの有無を選べるようにした @2021/6/11 Yasuomi Sakai
        {
            m_Playable.Play(kDefaultStateName);
        }
        return false;
    }

    public void Pause()
    {
        if (m_Clip != null && isPlaying)
        {
            m_Playable.Pause(0);//雑実装で０番目のみ @2021/6/11 Yasuomi Sakai
        }
    }

    public void Resume()
    {
        if (m_Clip != null && isPlaying)
        {
            m_Playable.Resume(0);//雑実装で０番目のみ @2021/6/11 Yasuomi Sakai
        }
    }

    public void AddState(AnimationClip clip, string name)
    {
        LegacyClipCheck(clip);
        Kick();
        if (m_Playable.AddClip(clip, name))
        {
            RebuildStates();
        }

    }

    public void RemoveState(string name)
    {
        if (m_Playable.RemoveClip(name))
        {
            RebuildStates();
        }
    }

    public void ppPlay(string stateName)
    {
        if (!m_Initialized)
        {
            Initialize();
        }
        //	Debug.Log("!!! Play " + stateName);
        animator.enabled = true;
        Kick();
        m_Playable.Play(stateName);
    }

    public bool Play(string stateName)
    {
        if (!m_Initialized)
        {
            Initialize();
        }
        //	Debug.Log("!!! Play " + stateName);
        animator.enabled = true;
        Kick();
        return m_Playable.Play(stateName);
    }

    public bool PlayDefault()
    {
        if (!m_Initialized)
        {
            Initialize();
        }
        animator.enabled = true;
        Kick();
        return m_Playable.Play(kDefaultStateName);
    }

    public void PlayEnterExit(int milliseconds)
    {
        Play("Enter");
        Observable
            .Timer(TimeSpan.FromMilliseconds(milliseconds))
            .Subscribe(_ => Play("Exit"));
    }

    public void PlayQueued(string stateName, QueueMode queueMode)
    {
        animator.enabled = true;
        Kick();
        m_Playable.PlayQueued(stateName, queueMode);
    }

    public void RemoveClip(AnimationClip clip)
    {
        if (clip == null)
            throw new System.NullReferenceException("clip");

        if (m_Playable.RemoveClip(clip))
        {
            RebuildStates();
        }

    }

    public void Rewind()
    {
        Kick();
        m_Playable.Rewind();
    }

    public void Rewind(string stateName)
    {
        Kick();
        m_Playable.Rewind(stateName);
    }

    public State GetState(string stateName)
    {
        SimpleAnimationPlayable.IState state = m_Playable.GetState(stateName);
        if (state == null)
            return null;

        return new StateImpl(state, this);
    }

    public IEnumerable<State> GetStates()
    {
        return new StateEnumerable(this);
    }
}
