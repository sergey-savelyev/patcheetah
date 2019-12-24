using System.Collections.Generic;

namespace Modelee.Tests.Models
{
    public interface ITestModel<TExtraInfo, TInnerExtraInfo>
        where TExtraInfo : class, IExtraInfo<TInnerExtraInfo>, new()
        where TInnerExtraInfo : class, IInnerExtraInfo, new()
    {
        string Id { get; set; }

        int Counter { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        TExtraInfo ExtraInfo { get; set; }

        string OnlyModelString { get; set; }

        List<TExtraInfo> ExtraInfoList { get; set; }

        TExtraInfo[] ExtraInfoArray { get; set; }

        int[] IntegerArray { get; set; }
    }
}
