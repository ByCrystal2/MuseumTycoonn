using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Playables;

public class TutoSceneTimeLine : MonoBehaviour
{
    private PlayableDirector currentActiveCutscene;
    public VideoPlayer videoPlayer;
    public Text SubtitleText;
    public CanvasGroup SubtitleCanvas;
    public List<GameObject> CloseEndVideo;

    private SubtitleData subtitleData;

    void Start()
    {
        List<Vector2> seconds = new List<Vector2>() 
        { 
            new Vector2(0.080f, 2.720f),
            new Vector2(2.720f, 5.320f),
            new Vector2(5.320f, 8.080f),
            new Vector2(8.720f, 11.52f),
            new Vector2(13.04f, 15.52f),
            new Vector2(15.52f, 18.16f),
            new Vector2(18.48f, 20.1f),
            new Vector2(20.1f , 23.6f),
            new Vector2(23.6f, 26.08f),
            new Vector2(26.08f, 26.64f),
            new Vector2(26.99f, 28.75f),
            new Vector2(28.75f, 31.35f),
            new Vector2(31.35f, 32.67f),
            new Vector2(32.912f, 35.152f),
            new Vector2(35.152f,37.792f),
            new Vector2(37.792f,40.352f),
            new Vector2(40.832f,43.632f),
            new Vector2(45.072f,47.832f),
            new Vector2(47.832f,49.632f)
        };

        List<string> texts = new List<string>()
        {
            "Welcome to a Kingdom where time stands",
            "still, a land of splendour and",
            "power lost in the pages of history.",
            "This is the Kingdom of the Golden Museum.",
            "And here we are in the heart of the",
            "Kingdom, in the bustling streets,",
            "the clatter of horse-drawn carriages, the",
            "shouts of market vendors, and the",
            "footsteps of royal guards echo",
            "through.",
            "you are here to take on the task of",
            "preserving the kingdom's rich history and",
            "cultural heritage.",
            "The King entrusts you with managing the",
            "Kingdom's greatest museum. This",
            "museum will house the past,",
            "present and future of the Kingdom.",
            "Welcome. Are you ready to write the",
            "story of the Kingdom together?"
        };
        subtitleData = new();
        subtitleData.BeginEndSeconds = seconds;
        subtitleData.Text = texts;

        videoPlayer.playOnAwake = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.Play();

        //currentActiveCutscene = gameObject.GetComponent<PlayableDirector>();
        //currentActiveCutscene.stopped += OnPlayableDirectorStopped;
    }

    private void OnVideoEnd(VideoPlayer source)
    {
        foreach (var item in CloseEndVideo)
            item.gameObject.SetActive(false);
        TutorialLevelManager.instance.OnEndFlyCutscene();
    }

    void OnPlayableDirectorStopped(PlayableDirector director)
    {
        Debug.Log("Timeline has ended.");
        OnEndEnterCutscene();
    }

    public void OnEndEnterCutscene()
    {
        //currentActiveCutscene.stopped -= OnPlayableDirectorStopped;
        //TutorialLevelManager.instance.OnEndFlyCutscene();
    }

    bool endVideo = false;
    private void OnNewFrame()
    {
        if (endVideo)
            return;

        double currentTime = videoPlayer.time;
        if(!endVideo && currentTime > 49.7f)
        {
            endVideo = true;
            OnVideoEnd(null);
            return;
        }

        // Access the current time in seconds
        bool active = false;
        foreach (var item in subtitleData.BeginEndSeconds)
        { 
            if (currentTime > item.x && currentTime < item.y)
            {
                active = true;
                SubtitleText.text = subtitleData.Text[subtitleData.BeginEndSeconds.IndexOf(item)];
                if (!SubtitleCanvas.gameObject.activeSelf)
                {
                    SubtitleCanvas.gameObject.SetActive(true);
                    StopAllCoroutines();
                    StartCoroutine(FadeIn(SubtitleCanvas));
                }
                else
                {
                    if (!fadein)
                    {
                        StopAllCoroutines();
                        StartCoroutine(FadeIn(SubtitleCanvas));
                    }
                }
            }
        }
        if (!active)
        {
            if (SubtitleCanvas.gameObject.activeSelf)
            {
                if (!fadeout)
                {
                    StopAllCoroutines();
                    StartCoroutine(FadeOut(SubtitleCanvas));
                }
            }
            else
            {
                SubtitleText.text = "";
            }
        }
    }

    void Update()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            OnNewFrame();
        }
    }

    bool fadein;
    bool fadeout;
    IEnumerator FadeOut(CanvasGroup c)
    {
        fadeout = true;
        while (c.alpha > 0)
        {
            c.alpha -= Time.deltaTime * 3f;
            if (c.alpha < 0)
                c.alpha = 0;

            yield return new WaitForEndOfFrame();
        }

        c.alpha = 0;
        c.gameObject.SetActive(false);
        fadeout = false;
    }

    IEnumerator FadeIn(CanvasGroup c)
    {
        fadein = true;
        while (c.alpha < 1)
        {
            c.alpha += Time.deltaTime * 3f;
            if (c.alpha > 1)
                c.alpha = 1;

            yield return new WaitForEndOfFrame();
        }

        c.alpha = 1;
        fadein = false;
    }

    public struct SubtitleData
    {
        public List<Vector2> BeginEndSeconds;
        public List<string> Text;
    }
}
