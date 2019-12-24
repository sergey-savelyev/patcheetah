namespace Modelee.Tests.Models
{
    public interface IExtraInfo<TInnerExtraInfo> where TInnerExtraInfo : class, IInnerExtraInfo, new()
    {
        string Id { get; set; }

        string Description { get; set; }

        TInnerExtraInfo InnerExtraInfo { get; set; }
    }
}
