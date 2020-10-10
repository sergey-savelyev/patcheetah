using System;
using System.Collections.Generic;
using System.Text;

namespace Patcheetah.Tests.Customization
{
    public class Route
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string StartPoint { get; set; }

        public string DestinationPoint { get; set; }

        [RoundValue(2)]
        public double Distance { get; set; }
    }
}
