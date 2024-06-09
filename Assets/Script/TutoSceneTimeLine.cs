using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TutoSceneTimeLine : MonoBehaviour
{
    private PlayableDirector currentActiveCutscene;
    // Start is called before the first frame update
    void Start()
    {
        currentActiveCutscene = gameObject.GetComponent<PlayableDirector>();
        currentActiveCutscene.stopped += OnPlayableDirectorStopped;
    }

    void OnPlayableDirectorStopped(PlayableDirector director)
    {
        Debug.Log("Timeline has ended.");
        OnEndEnterCutscene();
    }

    public void OnEndEnterCutscene()
    {
        currentActiveCutscene.stopped -= OnPlayableDirectorStopped;
        TutorialLevelManager.instance.OnEndFlyCutscene();
    }
}
