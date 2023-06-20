/// Rapid Video Scrubber Main Controller
/// Copyright (c) StokedOn LLC. All rights reserved.
/// Written by Sacha Berwin - StokedOnIt Software 2023


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.SceneManagement;



public class MainController : MonoBehaviour
{

    public Animator MainAnimation;
    public RawImage VideoImage;
    public int CurrentSenario;
    public GameObject NextButton;
    public UnityEngine.UI.Slider Slider;
    public UnityEngine.UI.Image FlagImage;
    public List<Senario> SenarioList;
    public Texture2D StartFrame;
    public GameObject AnteriorView;
    public GameObject LateralView;
    public GameObject DepthView;
    public UnityEngine.UI.Image NavImage;
    public GameObject Disclaimer;
    public GameObject MainMenu;

    float currentVideoTime = 0;
    int currentStopPoint = 0;
    string CurrentScenarioName = "Scenario1";
    bool isPlaying;
    int ScenarioImageSetCount = 3;
    int currentImageSet = 2;

    bool isAR = false;


    [Serializable]
    public class Senario
    {
        public string Name;
        public float AnimationToVideoSpeedRatio;
        public ScenarioStopPoints[] StopPoints;
        public float AnimationStartFudge;
        public float AnimationEndFudge;
        public ScenarioImageSet[] VideoImageSets;
        public GameObject Model;
        public Sprite NavImage;
    }
    [Serializable]
    public class ScenarioStopPoints
    {
        public float Percentage;
        public Texture2D Image;
        public Rect ImagePosition;
        public Rect ClickPosition;
        public bool ImageIsLeftOfMarker;
        public bool IsReverse;
    }
    [Serializable]
    public class ScenarioImageSet
    {
        public Texture2D[] Images;
        public float VideoStartFudge;
        public float VideoEndFudge;
    }
    public Senario GetScenario(string name)
    {
        if (SenarioList.Any(s => s.Name == name))
        {
            return SenarioList.First(s => s.Name == name);
        }
        else
            return null;
    }
    public Senario GetCurrentScenario()
    {
        return GetScenario(CurrentScenarioName);
    }
    public void ClearScenarioAnimation()
    {
        foreach (var scenario in SenarioList)
        {
            MainAnimation.SetBool(scenario.Name, false);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        Play("Scenario1");
        VideoImage.texture = StartFrame;
        if(SceneManager.GetActiveScene() != null && SceneManager.GetActiveScene().name == "AR")
            isAR = true;
    }


    // Update is called once per frame
    void Update()
    {

    }



    public void StartPlayback()
    {
        NextButton.SetActive(false);
        PlayNext();
    }
    public void Play(string name)
    {
        SwitchImageSet(0);
        Slider.value = Slider.minValue;
        ClearScenarioAnimation();
        if (CurrentScenarioName != "")
            GetCurrentScenario().Model.SetActive(false);
        CurrentScenarioName = name;
        GetCurrentScenario().Model.SetActive(true);
        MainAnimation = GetCurrentScenario().Model.GetComponent<Animator>();
        currentStopPoint = 0;
        var texture = GetCurrentScenario().StopPoints[currentStopPoint].Image;
        FlagImage.rectTransform.sizeDelta = new Vector2(texture.width * 0.1f, texture.height * 0.1f);
        var currentPosition = FlagImage.rectTransform.position;
        FlagImage.rectTransform.position = new Vector3(Slider.targetGraphic.rectTransform.position.x, currentPosition.y, currentPosition.z);
        FlagImage.sprite = Sprite.Create(GetCurrentScenario().StopPoints[currentStopPoint].Image, new Rect(0, 0, texture.width, texture.height), new Vector2());
        FlagImage.gameObject.SetActive(true);

        var scenario = GetCurrentScenario();
        NavImage.sprite = scenario.NavImage;

        if (scenario.StopPoints[currentStopPoint].ClickPosition.x != 0 || scenario.StopPoints[currentStopPoint].ClickPosition.y != 0)
        {
            var pos = scenario.StopPoints[currentStopPoint].ClickPosition.position;
            var size = scenario.StopPoints[currentStopPoint].ClickPosition.size;
            var rectTransform = NextButton.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = pos;
        }
        else
        {
            var rectTransform = NextButton.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2();
            rectTransform.anchoredPosition = new Vector2();
        }
        if (scenario.StopPoints[currentStopPoint].ImagePosition.x != 0 || scenario.StopPoints[currentStopPoint].ImagePosition.y != 0)
        {
            FlagImage.rectTransform.anchoredPosition = new Vector2(scenario.StopPoints[currentStopPoint].ImagePosition.x, scenario.StopPoints[currentStopPoint].ImagePosition.y);
        }
        else
        {
            var sliderPosition = Slider.transform.GetComponent<RectTransform>();
            if (Slider.value < .5f)
                FlagImage.rectTransform.position = new Vector3(Slider.targetGraphic.rectTransform.position.x, sliderPosition.position.y, currentPosition.z);
            else
                FlagImage.rectTransform.position = new Vector3(Slider.targetGraphic.rectTransform.position.x - (texture.width * 2f), sliderPosition.position.y, currentPosition.z);
        }
        //var videoValue = scenario.VideoImageSets[currentImageSet].VideoStartFudge;
        //var imageCount = scenario.VideoImageSets[currentImageSet].Images.Count();
        //var imageToDisplay = (int)Math.Round(imageCount * videoValue, 0);
        //VideoImage.texture = scenario.VideoImageSets[currentImageSet].Images[imageToDisplay];
        //var animValue = scenario.AnimationStartFudge;
        //MainAnimation.SetFloat("Progress", animValue);
        //MainAnimation.SetBool("Success", true);
        //currentVideoTime = GetCurrentScenario().StopPoints[currentStopPoint].Percentage;
        //NextButton.SetActive(true);
        //if (Slider.value == 0)
        //{
        //    VideoImage.texture = StartFrame;
        //}
    }
    public void PlayNext()
    {
        isPlaying = true;
        currentStopPoint++;
        if (currentStopPoint >= GetCurrentScenario().StopPoints.Length)
        {
            Play(CurrentScenarioName);
            return;
        }
        FlagImage.gameObject.SetActive(false);
        FlagImage.gameObject.SetActive(false);
        //Video.Play();
        SliderUpdated(Slider);
    }

    public void SliderUpdated(UnityEngine.UI.Slider slider)
    {
        var scenario = GetCurrentScenario();
        var stopPoint = scenario.StopPoints[currentStopPoint];
        var percentageToStopAt = stopPoint.Percentage;

        if (!isPlaying)
        {
            slider.value = percentageToStopAt;
            return;
        }
        else if ((!stopPoint.IsReverse && slider.value >= percentageToStopAt) || (stopPoint.IsReverse && slider.value <= percentageToStopAt))
        {
            slider.value = percentageToStopAt;
            isPlaying = false;
            var texture = scenario.StopPoints[currentStopPoint].Image;
            var currentPosition = FlagImage.rectTransform.position;
            FlagImage.rectTransform.sizeDelta = new Vector2(texture.width * 0.1f, texture.height * 0.1f);

            if (scenario.StopPoints[currentStopPoint].ClickPosition.x != 0 || scenario.StopPoints[currentStopPoint].ClickPosition.y != 0)
            {
                var pos = scenario.StopPoints[currentStopPoint].ClickPosition.position;
                var size = scenario.StopPoints[currentStopPoint].ClickPosition.size;
                var rectTransform = NextButton.GetComponent<RectTransform>();
                rectTransform.sizeDelta = size;
                rectTransform.anchoredPosition = pos;
            }
            else
            {
                var rectTransform = NextButton.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2();
                rectTransform.anchoredPosition = new Vector2();
            }
            if (scenario.StopPoints[currentStopPoint].ImagePosition.x != 0 || scenario.StopPoints[currentStopPoint].ImagePosition.y != 0)
            {
                FlagImage.rectTransform.anchoredPosition = new Vector2(scenario.StopPoints[currentStopPoint].ImagePosition.x, scenario.StopPoints[currentStopPoint].ImagePosition.y);
            }
            else
            {
                var sliderPosition = Slider.transform.GetComponent<RectTransform>();
                if (!scenario.StopPoints[currentStopPoint].ImageIsLeftOfMarker)
                    FlagImage.rectTransform.position = new Vector3(Slider.targetGraphic.rectTransform.position.x, sliderPosition.position.y, currentPosition.z);
                else
                    FlagImage.rectTransform.position = new Vector3(Slider.targetGraphic.rectTransform.position.x - (texture.width * 2f), sliderPosition.position.y, currentPosition.z);
            }
            FlagImage.sprite = Sprite.Create(scenario.StopPoints[currentStopPoint].Image, new Rect(0, 0, texture.width, texture.height), new Vector2());
            FlagImage.gameObject.SetActive(true);
            //return;
        }

        var value = slider.value;
        var videoValue = scenario.VideoImageSets[currentImageSet].VideoStartFudge + (value * (scenario.VideoImageSets[currentImageSet].VideoEndFudge - scenario.VideoImageSets[currentImageSet].VideoStartFudge));
        var imageCount = scenario.VideoImageSets[currentImageSet].Images.Count() - 1;
        var imageToDisplay = (int)Math.Round(imageCount * videoValue,0);
        VideoImage.texture = scenario.VideoImageSets[currentImageSet].Images[imageToDisplay];
        var animValue = scenario.AnimationStartFudge + (value * (scenario.AnimationEndFudge - scenario.AnimationStartFudge));
        MainAnimation.SetFloat("Progress", animValue);
        if (Slider.value == 0)
        {
            VideoImage.texture = StartFrame;
        }

    }
    public void SwitchImageSet(int num)
    {
        currentImageSet = num;

        var scenario = GetCurrentScenario();
        var value = Slider.value;
        var videoValue = scenario.VideoImageSets[num].VideoStartFudge + (value * (scenario.VideoImageSets[num].VideoEndFudge - scenario.VideoImageSets[num].VideoStartFudge));
        var imageCount = scenario.VideoImageSets[num].Images.Count();
        var imageToDisplay = (int)Math.Round(imageCount * videoValue, 0);
        VideoImage.texture = scenario.VideoImageSets[num].Images[imageToDisplay];
        if (isAR)
            return;
        //Non AR Code
        if (currentImageSet == 0)
        {
            DepthView.SetActive(true);
            LateralView.SetActive(false);
        }
        else
        {
            DepthView.SetActive(false);
            LateralView.SetActive(true);
        }
    }
    public void SwitchNextImageSet()
    {
        var scenario = GetCurrentScenario();

        currentImageSet++;
        if (currentImageSet >= 3)
            currentImageSet = 1;


        SwitchImageSet(currentImageSet);
    }
    public void SwitchNextSecondaryImageSet()
    {
        if (currentImageSet == 2)
            return;
        var scenario = GetCurrentScenario();

        currentImageSet++;
        if (currentImageSet >= 2)
            currentImageSet = 0;


        SwitchImageSet(currentImageSet);
    }

    public void DisableObject(GameObject go)
    {
        go.SetActive(false);
    }

    //public void SwitchToAR()
    //{
    //    SceneManager.LoadScene("AR");
    //}
    //public void SwitchToMain()
    //{
    //    SceneManager.LoadScene("Main");
    //}

    public void BackToMainMenu()
    {
        Disclaimer.SetActive(true);
        MainMenu.SetActive(true);
    }

}
