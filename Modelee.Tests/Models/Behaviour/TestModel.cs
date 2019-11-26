using System.Collections.Generic;
using Modelee.Attributes;

namespace Modelee.Tests.Models.Pack1
{
    public class TestModel
    {
        // Key field
        // If we have this property into patch-request, we usually wanna modify existing object
        // If we don't have this property into patch-request, we usually wanna create new object
        // Expected patching behaviour: standard patching behaviour
        [KeyProperty]
        public string Id { get; set; }

        // Field that should be immutable for patching
        // Expected patching behaviour: should be ignored on patching
        [IgnoreOnPatching]
        public int Counter { get; set; }

        // Required field with standard patch behaviour
        // Expected behaviour: should be required on patching
        [RequiredField]
        public string Name { get; set; }

        // Not required field with standard patch behaviour
        // Expected behaviour: standard patching behaviour
        public string Description { get; set; }

        // Field that has special alias for view model and patch request
        // It also contains internal modelee behaviour
        // Expected patching behaviour: should be patched by "AdditionalInfo" alias using internal modelee behaviour model
        [UseModelee]
        [ViewModelName("AdditionalInfo")]
        public ExtraInfo ExtraInfo { get; set; }

        // Field that not included into view model, but it can be included into patch request
        // It also contains internal modelee behaviour, but this property just a string, so that attribute should be ignored
        // Expected patching behaviour: standard patching behaviour
        [UseModelee]
        [NotIncludedInViewModel]
        public string OnlyModelString { get; set; }

        public List<ExtraInfo> ExtraInfoList { get; set; }

        public ExtraInfo[] ExtraInfoArray { get; set; }

        public int[] IntegerArray { get; set; }
    }
}
