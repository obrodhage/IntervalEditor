using System;
using System.Collections.Generic;
using System.Linq;
using Dragginz.AudioTool.Scripts.Includes;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using Dragginz.AudioTool.Scripts.StepEditor.UI;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Object = UnityEngine.Object;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public class Track
    {
        private Action<int, uint> _callbackTrackClick;

        public List<List<int>> Intervals;
        
        public List<Region> Regions;

        public int Position;
        public ScriptableObjectInstrument Instrument;

        private bool muted;
        //public bool solo;

        private int curRegionIndex;
        private bool donePlaying;

        public InstrumentController InstrumentController;

        private UiTrack _trackUi;

        public void Init(int pos, ScriptableObjectInstrument inst)
        {
            Regions = new List<Region>();

            Position = pos;
            Instrument = inst;

            muted = false;
        }

        public void SetListener(UiTrack trackUi, Action<int, uint> callbackTrackClick)
        {
            _trackUi = trackUi;
            _callbackTrackClick = callbackTrackClick;

            _trackUi.OnClickTrackInfoEvent += OnTrackInfoClick;
            _trackUi.OnClickMuteEvent += OnMuteClick;
            _trackUi.OnClickDeleteEvent += OnDeleteClick;
        }

        public void UpdatePosition(int pos)
        {
            Position = pos;
            if (_trackUi != null) _trackUi.UpdateTrackId(Position);

            InstrumentController.UpdateParentName(Position + 1);

            foreach (var region in Regions)
            {
                region.UpdateTrackPosition(Position);
            }
        }

        public void UpdateInstrument(ScriptableObjectInstrument scriptableObjectInstrument)
        {
            Instrument = scriptableObjectInstrument;
            InstrumentController.UpdateInstrument(Instrument);
            InstrumentController.UpdateParentName(Position + 1);

            if (_trackUi != null)
            {
                _trackUi.UpdateInstrument(Instrument.name, Instrument.defaultColor);
                _trackUi.UpdateTrackId(Position);
            }
        }
    
        private void MuteTrackAndRegions()
        {
            muted = !muted;
            foreach (var r in Regions) {
                r.Mute(muted);
            }
            
            InstrumentController.Mute(muted);
            
            if (_trackUi != null) {
                _trackUi.ShowMuted(muted, Instrument.name+(muted ? " Muted" : ""));
            }
        }

        public void Remove()
        {
            if (_trackUi == null) return;

            _trackUi.OnClickTrackInfoEvent -= OnTrackInfoClick;
            _trackUi.OnClickMuteEvent -= OnMuteClick;
            _trackUi.OnClickDeleteEvent -= OnDeleteClick;

            _callbackTrackClick = null;

            InstrumentController.Remove();
            Object.Destroy(InstrumentController.GoParent);
            InstrumentController = null;

            _trackUi.Remove();
            _trackUi = null;
        }

        // EVENTS

        private void OnTrackInfoClick()
        {
            if (_callbackTrackClick != null) _callbackTrackClick.Invoke(Position, Globals.TrackInfo);
        }

        private void OnMuteClick()
        {
            MuteTrackAndRegions();
        }

        private void OnDeleteClick()
        {
            if (_callbackTrackClick != null) _callbackTrackClick.Invoke(Position, Globals.TrackDelete);
        }

        public void AddRegion(Region region)
        {
            Regions.Add(region);
            Regions = Regions.OrderBy(x => x.startPosBeats).ToList();
        }

        public Globals.MouseRegionBeatPos ValidateRegionPosAndSize(Globals.MouseRegionBeatPos regionBeatPos)
        {
            var numRegions = Regions.Count;
            var exitChecks = false;

            if (regionBeatPos.regionStartPos < 0) regionBeatPos.regionStartPos = 0;
            
            // check if region is within other region
            for (var i = 0; i < numRegions; ++i)
            {
                var r = Regions[i];
                if (regionBeatPos.regionStartPos >= r.startPosBeats) {
                    if (regionBeatPos.regionStartPos + regionBeatPos.numBeats <= r.startPosBeats + r.beats) {
                        regionBeatPos.posIsValid = false;
                        //Debug.LogError("regionMarker within other region!");
                        exitChecks = true;
                        break;
                    }
                }
            }

            if (exitChecks) return regionBeatPos;
            
            // check if start of region is overlapping other region
            for (var i = 0; i < numRegions; ++i)
            {
                var r = Regions[i];
                if (regionBeatPos.regionStartPos >= r.startPosBeats) {
                    if (regionBeatPos.regionStartPos <= r.startPosBeats + r.beats)
                    {
                        //Debug.LogWarning("overlap: "+regionBeatPos.regionStartPos+" :: "+r.startPosBeats+", "+(r.startPosBeats+r.beats));
                        
                        regionBeatPos.regionStartPos = r.startPosBeats + r.beats;

                        // not the last region?
                        if (i < (numRegions - 1))
                        {
                            var rNext = Regions[i+1];
                            if (regionBeatPos.regionStartPos + regionBeatPos.numBeats >= rNext.startPosBeats) {
                                regionBeatPos.numBeats = rNext.startPosBeats - regionBeatPos.regionStartPos;
                            }
                        }
                        
                        exitChecks = true;
                        break;
                    }
                }
            }
            
            if (exitChecks) return regionBeatPos;
            
            // check if end of region is overlapping other region
            for (var i = 0; i < numRegions; ++i)
            {
                var r = Regions[i];
                if (regionBeatPos.regionStartPos + regionBeatPos.numBeats >= r.startPosBeats) {
                    if (regionBeatPos.regionStartPos + regionBeatPos.numBeats <= r.startPosBeats + r.beats)
                    {
                        regionBeatPos.regionStartPos = r.startPosBeats - regionBeatPos.numBeats;
                        if (regionBeatPos.regionStartPos < 0) {
                            regionBeatPos.regionStartPos = 0;
                        }

                        regionBeatPos.numBeats = r.startPosBeats - regionBeatPos.regionStartPos;
                        break;
                    }
                }
            }
            
            return regionBeatPos;
        }

        public void PrepareForPlayback()
        {
            if (Regions.Count <= 0)
            {
                donePlaying = true;
                return;
            }

            foreach (var region in Regions) {
                region.isPlaying = false;
            }
            
            donePlaying = false;
            curRegionIndex = 0;
        }
        
        /*public void StartPlayback(double startDspTime, double curDspTime, int numInstrumentsSoloed)
        {
            var region = regions[curRegionIndex];

            if (instrument.type == InstrumentType.Chord)
            {
                instrumentController.PlayIntervals(region.key, Intervals[region.interval], region.playbackSettings,
                    numInstrumentsSoloed,
                    startDspTime);

                //Debug.Log("instrument "+instrument.name+" - start playback - key "+region.key);
                //curRegionIndex++;
            }
            else
            {
                instrumentController.StartPlayback(region.PianoRoll, region.playbackSettings,
                    numInstrumentsSoloed,
                    startDspTime, startDspTime);
            }
        }*/
        
        public void UpdatePlayback(double startDspTime, double curDspTime, int numInstrumentsSoloed)
        {
            if (donePlaying) return;
            
            var region = Regions[curRegionIndex];
            if (!region.isPlaying)
            {
                if (curDspTime >= startDspTime + region.regionStartTime)
                {
                    StartRegionPlayback(startDspTime, curDspTime, numInstrumentsSoloed);
                }
            }
            else
            {
                if (curDspTime >= startDspTime + region.regionEndTime)
                {
                    if (region.playbackSettings.Type == Globals.RegionTypeLoop) //InstrumentController.Settings.CanLoop
                    {
                        //Debug.Log("skip to end");
                        InstrumentController.SkipToEndBeat(curDspTime); // let loopers ring out
                    }

                    PrepareFoNextRegionPlayback();
                    if (donePlaying) return;
                    
                    region = Regions[curRegionIndex];
                    if (curDspTime >= startDspTime + region.regionStartTime)
                    {
                        StartRegionPlayback(startDspTime, curDspTime, numInstrumentsSoloed);
                    }
                }
                else
                {
                    if (region.playbackSettings.Type == Globals.RegionTypeLoop) //Instrument.type == InstrumentType.Looper)
                    {
                        if (!InstrumentController.Settings.CanLoop) return;
                        if (curDspTime >= InstrumentController.LoopDspTime)
                        {
                            Debug.Log("looping back!");
                            InstrumentController.LoopBack(curDspTime);
                        }
                    }
                    else
                    {
                        InstrumentController.UpdatePlayback(curDspTime, region.playbackSettings, numInstrumentsSoloed);
                    }       
                }
            }
        }

        private void PrepareFoNextRegionPlayback()
        {
            if ((curRegionIndex+1) >= Regions.Count)
            {
                donePlaying = true;
            }
            else
            {
                curRegionIndex++;
            }
        }

        private void StartRegionPlayback(double startDspTime, double curDspTime, int numInstrumentsSoloed)
        {
            /*if (curRegionIndex >= regions.Count)
            {
                donePlaying = true;
                instrumentController.StopAllSources();
                return;
            }*/

            var region = Regions[curRegionIndex];
            region.isPlaying = true;
            
            if (region.playbackSettings.Type == Globals.RegionTypeLoop) //Instrument.type == InstrumentType.Looper)
            {
                InstrumentController.PlayIntervals(
                    region.playbackSettings.Key,
                    Intervals[region.playbackSettings.Interval],
                    region.playbackSettings,
                    numInstrumentsSoloed,
                    startDspTime, curDspTime);

                //Debug.Log("instrument "+instrument.name+" - start playback - key "+region.key);
            }
            else
            {
                InstrumentController.StartPlayback(region.PianoRoll, region.playbackSettings,
                    numInstrumentsSoloed,
                    startDspTime, curDspTime);
            }
        }
        
        public void StopPlayback()
        {
            donePlaying = true;
            curRegionIndex = 0;

            InstrumentController.StopAllSources();
            
            foreach (var region in Regions) {
                region.isPlaying = false;
            }
        }
    }
}
