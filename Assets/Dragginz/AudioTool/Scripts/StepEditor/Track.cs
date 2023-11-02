using System;
using System.Collections.Generic;
using System.Linq;
using Dragginz.AudioTool.Scripts.Includes;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using Dragginz.AudioTool.Scripts.StepEditor.UI;
using Object = UnityEngine.Object;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public class Track
    {
        private Action<int, uint> _callbackTrackClick;

        public List<List<int>> Intervals;
        
        public List<Region> Regions;

        public int Position;
        public int EndRegionBeat;
        public ScriptableObjectInstrument Instrument;

        private int curRegionIndex;
        private bool donePlaying;

        public InstrumentController InstrumentController;

        private UiTrack _trackUi;

        // Getters

        public int ReverbFilter { get; private set; }
        public bool Muted { get; private set; }

        public void Init(int pos, ScriptableObjectInstrument inst)
        {
            Regions = new List<Region>();

            Position = pos;
            EndRegionBeat = 0;
            Instrument = inst;

            Muted = false;
            ReverbFilter = 0;
        }

        public void SetListener(UiTrack trackUi, Action<int, uint> callbackTrackClick)
        {
            _trackUi = trackUi;
            _callbackTrackClick = callbackTrackClick;

            _trackUi.OnClickTrackInfoEvent += OnTrackInfoClick;
            _trackUi.OnClickMuteEvent += OnMuteClick;
            _trackUi.OnClickDeleteEvent += OnDeleteClick;
        }

        public void UpdateReverbFilter(int value)
        {
            ReverbFilter = value;
            InstrumentController.SetReverbFilter(ReverbFilter);
            if (_trackUi != null) _trackUi.SetReverbFilter(ReverbFilter);
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
    
        public void MuteTrackAndRegions(bool mute)
        {
            Muted = mute;
            foreach (var r in Regions) {
                r.Mute(Muted);
            }
            
            InstrumentController.Mute(Muted);
            
            if (_trackUi != null) {
                _trackUi.ShowMuted(Muted, Instrument.name+(Muted ? " Muted" : ""));
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
            MuteTrackAndRegions(!Muted);
        }

        private void OnDeleteClick()
        {
            if (_callbackTrackClick != null) _callbackTrackClick.Invoke(Position, Globals.TrackDelete);
        }

        public void AddRegion(Region region)
        {
            Regions.Add(region);
            Regions = Regions.OrderBy(x => x.StartPosBeats).ToList();
        }

        public Globals.MouseRegionBeatPos ValidateRegionPosAndSize(Globals.MouseRegionBeatPos regionBeatPos)
        {
            var numRegions = Regions.Count;
            var exitChecks = false;

            if (regionBeatPos.RegionStartPos < 0) regionBeatPos.RegionStartPos = 0;
            
            // check if region is within other region
            for (var i = 0; i < numRegions; ++i)
            {
                var r = Regions[i];
                if (regionBeatPos.RegionStartPos >= r.StartPosBeats) {
                    if (regionBeatPos.RegionStartPos + regionBeatPos.NumBeats <= r.StartPosBeats + r.Beats) {
                        regionBeatPos.PosIsValid = false;
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
                if (regionBeatPos.RegionStartPos >= r.StartPosBeats) {
                    if (regionBeatPos.RegionStartPos <= r.StartPosBeats + r.Beats)
                    {
                        //Debug.LogWarning("overlap: "+regionBeatPos.regionStartPos+" :: "+r.startPosBeats+", "+(r.startPosBeats+r.beats));
                        
                        regionBeatPos.RegionStartPos = r.StartPosBeats + r.Beats;

                        // not the last region?
                        if (i < (numRegions - 1))
                        {
                            var rNext = Regions[i+1];
                            if (regionBeatPos.RegionStartPos + regionBeatPos.NumBeats >= rNext.StartPosBeats) {
                                regionBeatPos.NumBeats = rNext.StartPosBeats - regionBeatPos.RegionStartPos;
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
                if (regionBeatPos.RegionStartPos + regionBeatPos.NumBeats >= r.StartPosBeats) {
                    if (regionBeatPos.RegionStartPos + regionBeatPos.NumBeats <= r.StartPosBeats + r.Beats)
                    {
                        regionBeatPos.RegionStartPos = r.StartPosBeats - regionBeatPos.NumBeats;
                        if (regionBeatPos.RegionStartPos < 0) {
                            regionBeatPos.RegionStartPos = 0;
                        }

                        regionBeatPos.NumBeats = r.StartPosBeats - regionBeatPos.RegionStartPos;
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
                region.IsPlaying = false;
            }
            
            EndRegionBeat = Regions[^1].StartPosBeats + Regions[^1].Beats;
            
            donePlaying = false;
            curRegionIndex = 0;
        }
        
        public void UpdatePlayback(double startDspTime, double curDspTime, int numInstrumentsSoloed)
        {
            if (donePlaying) return;
            
            var region = Regions[curRegionIndex];
            if (!region.IsPlaying)
            {
                if (curDspTime >= startDspTime + region.RegionStartTime)
                {
                    StartRegionPlayback(startDspTime, curDspTime, numInstrumentsSoloed);
                }
            }
            else
            {
                if (curDspTime >= startDspTime + region.RegionEndTime)
                {
                    if (region.PlaybackSettings.Type == (int)InstrumentType.Looper)
                    {
                        //Debug.Log("skip to end");
                        InstrumentController.SkipToEndBeat(curDspTime); // let loopers ring out
                    }

                    PrepareFoNextRegionPlayback();
                    if (donePlaying) return;
                    
                    region = Regions[curRegionIndex];
                    if (curDspTime >= startDspTime + region.RegionStartTime)
                    {
                        StartRegionPlayback(startDspTime, curDspTime, numInstrumentsSoloed);
                    }
                }
                else
                {
                    if (region.PlaybackSettings.Type == (int)InstrumentType.Looper)
                    {
                        if (curDspTime >= InstrumentController.LoopDspTime)
                        {
                            //Debug.Log("loop");
                            InstrumentController.LoopBack(curDspTime);
                        }
                    }
                    else
                    {
                        InstrumentController.UpdatePlayback(curDspTime, region.PlaybackSettings, numInstrumentsSoloed);
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
            var region = Regions[curRegionIndex];
            region.IsPlaying = true;
            
            if (region.PlaybackSettings.Type == (int)InstrumentType.Looper)
            {
                InstrumentController.PlayIntervals(
                    region.PlaybackSettings.Key,
                    Intervals[region.PlaybackSettings.Interval],
                    region.PlaybackSettings,
                    numInstrumentsSoloed,
                    startDspTime, curDspTime);

                //Debug.Log("instrument "+instrument.name+" - start playback - key "+region.key);
            }
            else
            {
                InstrumentController.StartPlayback(region.PianoRoll, region.PlaybackSettings,
                    numInstrumentsSoloed,
                    startDspTime, curDspTime);
            }
        }
        
        public void StopPlayback()
        {
            //Debug.Log("StopPlayback");
            
            donePlaying = true;
            curRegionIndex = 0;

            InstrumentController.StopAllSources();
            
            foreach (var region in Regions) {
                region.IsPlaying = false;
            }
        }
    }
}
