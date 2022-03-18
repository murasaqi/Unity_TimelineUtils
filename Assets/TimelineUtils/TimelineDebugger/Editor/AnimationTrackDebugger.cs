using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace TimelineUtils
{
    public class AnimationTrackDebugger : EditorWindow
    {
// 1
        [MenuItem("Tools/AnimationTrackDebugger")]
        public static void ShowExample()
        {
            // 2
            AnimationTrackDebugger wnd = GetWindow<AnimationTrackDebugger>();
            wnd.titleContent = new GUIContent("TimelineDebugger");
        }

        private List<Toggle> trackToggleList;
        private PlayableDirector director;
        private List<TrackAsset> trackAssets;
        private double preveoutDirectorTime = 0f;

        private ScrollView clipInfoLabel;

        // private StringBuilder stringBuilder;
        private ScrollView toggleScrollView;

        public void OnEnable()
        {
            // stringBuilder = new StringBuilder();
            // 3
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;



            // 5
            // Import UXML
            var visualTree = Resources.Load<VisualTreeAsset>
                ("TimelineDebuggerEditor");
            VisualElement labelFromUXML = visualTree.CloneTree();
            root.Add(labelFromUXML);


            // var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>
            //     ("Assets/TimelineDebugger/Editor/TimelineDebuggerEditor.uss");
            // VisualElement labelWithStyle = new Label("Hello World! With Style");
            // labelWithStyle.styleSheets.Add(styleSheet);
            // root.Add(labelWithStyle);



            clipInfoLabel = root.Q<ScrollView>("ClipInfo");
            var timlineField = root.Q<ObjectField>("PlayableDirectorField");
            toggleScrollView = root.Q<ScrollView>("TrackListView");
            timlineField.objectType = typeof(PlayableDirector);
            timlineField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {

                toggleScrollView.Clear();

                if (timlineField.value != null)
                {
                    trackToggleList = new List<Toggle>();
                    director = timlineField.value as PlayableDirector;
                    var playableAsset = director.playableAsset as TimelineAsset;
                    trackAssets = playableAsset.GetOutputTracks().ToList();
                    foreach (var track in trackAssets)
                    {
                        var toggle = new Toggle($"{track.name} {track.GetType()}");
                        toggle.style.flexDirection = FlexDirection.RowReverse;
                        trackToggleList.Add(toggle);
                        toggleScrollView.Add(toggle);
                    }
                }
                else
                {
                    trackToggleList.Clear();
                }



            });
            // root.Q<ObjectField>()

            // 6
            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its
            // children.

        }


        private string GetClipInfoByTime(double time, double offsetTime, TrackAsset trackAsset, string indent = "")
        {
            var builder = new StringBuilder();

            foreach (var clip in trackAsset.GetClips())
            {
                var start = clip.start + offsetTime;
                var end = clip.end + offsetTime;
                if (start <= time && time <= end)
                {

                    var frameRate = clip.animationClip.frameRate;
                    // stringBuilder.AppendLine($"{track.GetType()}: {clip.displayName}");



                    // var currentMotionDuration = clip.duration;

                    if (clip.asset.GetType() == typeof(AnimationPlayableAsset))
                    {
                        var diff = Mathf.Max((float) (time - clip.start), 0f);
                        // var animationPlayableAsset = clip.asset as AnimationPlayableAsset;
                        // var source = animationPlayableAsset.clip;
                        // var totalDuration = source.length;
                        builder.AppendLine(
                            $"{indent}{clip.displayName}\n {indent}{indent}current: {clip.clipIn + offsetTime + diff}sec  {(int) ((clip.clipIn + offsetTime + diff) * frameRate)}frame");

                    }
                    // builder.Append($"{indent}{clip.displayName}");
                }
            }

            return builder.ToString();
        }

        private void OnInspectorUpdate()
        {

            if (director != null)
            {
                if (director.time != preveoutDirectorTime)
                {
                    clipInfoLabel.Clear();
                    var count = 0;
                    // stringBuilder.Clear();
                    // stringBuilder.AppendLine($"time: {director.time}");
                    clipInfoLabel.Add(new Label($"time: {director.time}"));
                    foreach (var toggle in trackToggleList)
                    {
                        if (toggle.value)
                        {
                            var track = trackAssets[count];
                            if (track)
                            {
                                if (track.GetType() == typeof(ControlTrack))
                                {
                                    // stringBuilder.AppendLine(track.name);

                                    foreach (var clips in track.GetClips())
                                    {
                                        var controlPlayableAsset = clips.asset as ControlPlayableAsset;
                                        if (controlPlayableAsset.sourceGameObject.Resolve(director) != null)
                                        {
                                            var controlDirector = controlPlayableAsset.sourceGameObject
                                                .Resolve(director)
                                                .GetComponent<PlayableDirector>();
                                            // stringBuilder.AppendLine(controlDirector.name);
                                            clipInfoLabel.Add(new Label(controlDirector.name));
                                            var controlPlayableDirector =
                                                controlDirector.playableAsset as TimelineAsset;

                                            foreach (var grandChildTrack in controlPlayableDirector.GetOutputTracks())
                                            {
                                                clipInfoLabel.Add(new Label(grandChildTrack.name));
                                                var result = GetClipInfoByTime(director.time, clips.start,
                                                    grandChildTrack, "    ");
                                                if (result.Length > 0) clipInfoLabel.Add(new Label(result));
                                            }
                                        }
                                        // stringBuilder.AppendLine(grandChildTrack.name);
                                        // var result = GetClipInfoByTime(director.time, grandChildTrack);
                                        // if(result.Length > 0)stringBuilder.AppendLine(result);
                                    }

                                }
                                else
                                {
                                    var result = GetClipInfoByTime(director.time, 0, track);
                                    if (result.Length > 0) clipInfoLabel.Add(new Label(result));
                                }

                            }

                        }

                        count++;
                    }

                    // clipInfoLabel.Add(new Label(stringBuilder.ToString())); 
                }
            }

        }
    }
}