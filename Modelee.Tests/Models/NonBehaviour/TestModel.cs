using System.Collections.Generic;

namespace Modelee.Tests.Models.NonBehaviour
{
    public class TestModel : ITestModel<ExtraInfo, InnerExtraInfo>
    {
        public string Id { get; set; }

        public int Counter { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ExtraInfo ExtraInfo { get; set; }

        public string OnlyModelString { get; set; }

        public List<ExtraInfo> ExtraInfoList { get; set; }

        public ExtraInfo[] ExtraInfoArray { get; set; }

        public int[] IntegerArray { get; set; }
    }
}
