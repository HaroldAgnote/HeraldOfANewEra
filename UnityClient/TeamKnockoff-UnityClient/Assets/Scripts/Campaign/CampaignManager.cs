﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Assets.Scripts.Application;
using Assets.Scripts.Utilities.FileHandling;

namespace Assets.Scripts.Campaign {
    public class CampaignManager : MonoBehaviour {
        public static CampaignManager instance;

        public List<CampaignSequence> availableCampaigns;
        public CampaignSequence CurrentCampaignSequence { get; set; }

        private int mCurrentCampaignIndex;

        public int CurrentCampaignIndex {
            get { return mCurrentCampaignIndex; }
            set {
                if (mCurrentCampaignIndex != value) {
                    mCurrentCampaignIndex = value;
                }

                if (mCurrentCampaignIndex > FarthestCampaignIndex) {
                    FarthestCampaignIndex = mCurrentCampaignIndex;
                }
            }
        }

        private int mFarthestCampaignIndex;

        public int FarthestCampaignIndex {
            get { return mFarthestCampaignIndex; }
            private set {
                if (mFarthestCampaignIndex != value) {
                    mFarthestCampaignIndex = value;
                }
            }
        }

        public enum CampaignEvent {
            OpeningDialogue,
            CampaignMap,
            ClosingDialogue,
            SaveMenu,
            ChapterMenu
        }

        private CampaignEvent currentCampaignEvent;

        public List<CampaignData> CampaignDataSlots { get; private set; }
        public SortedDictionary<string, CampaignSequence> NamesToCampaignSequences { get; private set; }

        public bool CurrentCampaignIsFinished {
            get { return CurrentCampaignSequence.numberOfChapters == FarthestCampaignIndex; }
        }

        public bool CurrentCampaignSaved { get; private set; }

        public static string DataToString(CampaignData data) {
            var sequence = instance.NamesToCampaignSequences[data.CampaignName];
            return
                $"{sequence.campaignName}: Chapter {data.FarthestCampaignIndex + 1} - {sequence.chapterNames[data.FarthestCampaignIndex]}";
        }

        private void Awake() {
            //Check if instance already exists
            if (instance == null) {
                //if not, set instance to this
                instance = this;
            }

            //If instance already exists and it's not this:
            else if (instance != this) {
                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);
            }

            DontDestroyOnLoad(this.gameObject);
        }

        void Start() {
            NamesToCampaignSequences = new SortedDictionary<string, CampaignSequence>();
            foreach (var availableCampaign in availableCampaigns) {
                NamesToCampaignSequences.Add(availableCampaign.campaignName, availableCampaign);
            }

            CampaignDataSlots = CampaignDataFileHandler.LoadCampaignData();
        }

        public void LoadNextCampaignEvent() {

            // Load the next event given the current campaign event
            switch (currentCampaignEvent) {
                case CampaignEvent.OpeningDialogue:
                    LoadNextMap();
                    //LoadOpeningDialogue();
                    break;
                case CampaignEvent.CampaignMap:
                    // TODO: Change to Load Closing Dialogue
                    // and remove complete and save menu call

                    // LoadClosingDialogue();
                    // once I get Load Closing dialogue to work, comment out 2 below lines
                    //CompleteCurrentCampaignMap();
                    LoadClosingDialogue();
                    //LoadCampaignSaveMenu();
                    break;
                case CampaignEvent.ClosingDialogue:
                    CompleteCurrentCampaignMap();
                    LoadCampaignSaveMenu();
                    break;
                case CampaignEvent.SaveMenu:
                    LoadCampaignChapterMenu();
                    break;
                case CampaignEvent.ChapterMenu:
                    // TODO: Use Opening Dialogue Call and remove Map Call
                    // LoadOpeningDialogue();
                    // comment out below when LoadOpeningDialogue is opened
                    LoadNextMap();
                    break;
            }
        }

        public void StartNewCampaign(CampaignSequence newSequence) {
            CurrentCampaignSequence = newSequence;
            CurrentCampaignIndex = 0;
            FarthestCampaignIndex = 0;
            //TODO matt instead load first dialogue sequence
            LoadOpeningDialogue();
            //LoadCampaignChapterMenu();
        }

        public void LoadCampaign(CampaignData data) {
            CurrentCampaignSequence = NamesToCampaignSequences[data.CampaignName];
            CurrentCampaignIndex = data.CurrentCampaignIndex;
            FarthestCampaignIndex = data.FarthestCampaignIndex;
            LoadCampaignChapterMenu();
        }

        private void LoadCampaignChapterMenu() {
            currentCampaignEvent = CampaignEvent.ChapterMenu;
            SceneLoader.instance.GoToCampaignChapterMenu();
        }

        private void LoadNextMap() {
            currentCampaignEvent = CampaignEvent.CampaignMap;
            var nextMap = CurrentCampaignSequence.mapSequence[CurrentCampaignIndex].name;
            SceneLoader.SetParam(SceneLoader.LOAD_MAP_PARAM, nextMap);
            SceneLoader.SetParam(SceneLoader.GAME_TYPE_PARAM, GameManager.SINGLEPLAYER_GAME_TYPE);
            SceneLoader.SetParam(SceneLoader.SINGLEPLAYER_GAME_TYPE_PARAM, GameManager.CAMPAIGN_GAME_TYPE);
            SceneLoader.instance.GoToMap();
        }

        private void LoadOpeningDialogue() {
            //THIS IS WEHRE Matthews LOGIC COMES IN
            currentCampaignEvent = CampaignEvent.OpeningDialogue;
            var nextDialogue = CurrentCampaignSequence.preMapDialogueSequence[CurrentCampaignIndex].text;
            SceneLoader.SetParam(SceneLoader.LOAD_DIALOGUE_PARAM, nextDialogue);
            SceneLoader.SetParam(SceneLoader.GAME_TYPE_PARAM, GameManager.SINGLEPLAYER_GAME_TYPE);
            SceneLoader.SetParam(SceneLoader.SINGLEPLAYER_GAME_TYPE_PARAM, GameManager.CAMPAIGN_GAME_TYPE);
            SceneLoader.instance.GoToDialogue();
            //throw new NotImplementedException();
        }

        private void LoadClosingDialogue() {
            currentCampaignEvent = CampaignEvent.ClosingDialogue;
            var nextDialogue = CurrentCampaignSequence.postMapDialogueSequence[CurrentCampaignIndex].text;
            SceneLoader.SetParam(SceneLoader.LOAD_DIALOGUE_PARAM, nextDialogue);
            SceneLoader.SetParam(SceneLoader.GAME_TYPE_PARAM, GameManager.SINGLEPLAYER_GAME_TYPE);
            SceneLoader.SetParam(SceneLoader.SINGLEPLAYER_GAME_TYPE_PARAM, GameManager.CAMPAIGN_GAME_TYPE);
            SceneLoader.instance.GoToDialogue();
            //throw new NotImplementedException();
        }

        private void LoadCampaignSaveMenu() {
            currentCampaignEvent = CampaignEvent.SaveMenu;
            SceneLoader.instance.GoToCampaignSaveMenu();
        }

        public CampaignData SaveNewCampaign() {
            var newData = new CampaignData(CurrentCampaignSequence.campaignName, CurrentCampaignIndex, FarthestCampaignIndex);
            CampaignDataSlots.Add(newData);

            CampaignDataFileHandler.SaveCampaignData(newData);

            return newData;
        }

        public CampaignData SaveCampaign(CampaignData data) {

            // Overwrite existing data
            CampaignDataFileHandler.DeleteCampaignData(data);
            
            data.CurrentCampaignIndex = CurrentCampaignIndex;

            CampaignDataFileHandler.SaveCampaignData(data);

            CurrentCampaignSaved = true;
            return data;
        }

        private void CompleteCurrentCampaignMap() {
            CurrentCampaignSaved = false;
            CurrentCampaignIndex++;
        }
    }
}
