﻿using System;
using System.Collections.Generic;

namespace PlexDL.Common.Structures.AppOptions.Display.Grids.Music
{
    [Serializable]
    public class TrackDisplay : ColumnAdapter
    {
        public TrackDisplay()
        {
            DisplayColumns = new List<string>
            {
                "title", "year"
            };

            DisplayCaptions = new List<string>
            {
                "Track", "Year"
            };
        }
    }
}