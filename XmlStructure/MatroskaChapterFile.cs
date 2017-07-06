using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Bulletfish.MKVCommercialSkip.XmlStructure
{
    public class ChapterDisplay
    {
        [XmlElement("ChapterString")]
        public string Title { get; set; }

        [XmlElement("ChapterLanguage")]
        public string Language { get; set; }
    }

    public class ChapterAtom
    {
        [XmlIgnore]
        public TimeSpan Start { get; set; }

        [XmlIgnore]
        public TimeSpan? End { get; set; }

        [XmlElement("ChapterTimeStart")]
        public string ChapterTimeStart
        {
            get { return Start.ToString(); }
            set { }
        }

        [XmlElement("ChapterTimeEnd")]
        public string ChapterTimeEnd
        {
            get { return End.HasValue ? End.ToString() : ""; }
            set { }
        }

        public bool ShouldSerializeChapterTimeEnd()
        {
            return End.HasValue;
        }

        [XmlElement("ChapterDisplay")]
        public ChapterDisplay Display { get; set; }
        
        [XmlElement("ChapterFlagHidden")]
        public string ChapterFlagHidden { get; set; }

        [XmlElement("ChapterFlagEnabled")]
        public string ChapterFlagEnabled { get; set; }

        [XmlElement("ChapterUID")]
        public string ChapterUID { get; set; }
    }

    public class EditionEntry
    {
        [XmlElement("ChapterAtom")]
        public List<ChapterAtom> Chapters { get; set; }

        [XmlElement("EditionFlagDefault")]
        public string EditionFlagDefault { get; set; }

        [XmlElement("EditionFlagHidden")]
        public string EditionFlagHidden { get; set; }

        [XmlElement("EditionUID")]
        public string EditionUID { get; set; }
    }

    [XmlRoot("Chapters")]
    public class MatroskaChapterFile : List<EditionEntry>
    {
    }
}
