using isnd.tests;
using Xunit.Abstractions;

namespace yavscTests.Mandatory;

[Collection("Yavsc Server")]
[Trait("regression", "oui")]
public class Services : BaseTestContext, IClassFixture<WebServerFixture>
{
  public Services(ITestOutputHelper output, WebServerFixture fixture) : base(output, fixture)
  {

  }
}
