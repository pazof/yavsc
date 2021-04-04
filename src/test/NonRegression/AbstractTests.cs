using System;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Helpers;

namespace test
{
    [Collection("Yavsc Abstract tests")]
    [Trait("regression", "II")]
    public class AbstractTests
    {
        readonly ITestOutputHelper output;
        public AbstractTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        [Fact]
        public void UniquePathesAfterFileNameCleaning()
        {
            var name1 = "content:///scanned_files/2020-06-02/00.11.02.JPG";
            var name2 = "content:///scanned_files/2020-06-02/00.11.03.JPG";
            var cleanName1 = AbstractFileSystemHelpers.FilterFileName(name1);
            var cleanName2 = AbstractFileSystemHelpers.FilterFileName(name2);
            output.WriteLine($"{name1} => {cleanName1}");
            output.WriteLine($"{name2} => {cleanName2}");
            Assert.True(cleanName1 != cleanName2);
        }
    }
}
