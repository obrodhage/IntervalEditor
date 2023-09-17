using UnityEngine;

namespace Dragginz.AudioTool.Scripts.ScriptableObjects
{
    public enum PatternOctaves
    {
        One,
        Two,
        Three
    }

    public enum PatternStart
    {
        Beginning,
        End
    }
    
    public enum PatternEnd
    {
        Reverse,
        Reset
    }
    
    public enum PatternStartDirection
    {
        Forward,
        Back
    }
    
    public enum PatternType
    {
        SimpleArpeggio,
        ThreeNoteSteps,
        RootClimb
    }
    
    [CreateAssetMenu(fileName = "Pattern", menuName = "ScriptableObjects/Pattern", order = 3)]
    public class ScriptableObjectPattern : ScriptableObject
    {
        public uint sortOrder;
        
        public new string name;
        
        public PatternOctaves octaves;
        public PatternStart start;
        public PatternStartDirection startDirection;
        public PatternEnd end;
        public PatternType type;
    }
}

/*
    0   C
    1   C#
    2   D
    3   Eb
    4   E
    5   F
    6   Gb
    7   G
    8   G#
    9   A
    10  Bb
    11  B
    12  C
*/
    
/*private readonly int[][] _moodIntervals = {
    new [] { 0, 4, 7, 12 },      // Major C-E-G
    new [] { 0, 4, 7, 9, 12 },   // Major 6th C-E-G-A
    new [] { 0, 4, 7, 11, 12 },  // Major 7th C-E-G-B
    new [] { 0, 3, 7, 12 },      // Minor C-Eb-G
    new [] { 0, 3, 7, 9, 12 },   // Minor 6th C-Eb-G-A
    new [] { 0, 3, 7, 10, 12 },  // Minor 7th C-Eb-G-Bb
    new [] { 0, 4, 10, 12 },     // 7th C-E-Bb
    new [] { 0, 3, 6, 12 },      // Diminished C-Eb-Gb
    new [] { 0, 4, 8, 12 },      // Augmented C-E-G#
    new [] { 0, 2, 7, 12 },      // Sus 2 C-D-G
    new [] { 0, 5, 7, 12 }       // Sus 4 C-F-G
};*/