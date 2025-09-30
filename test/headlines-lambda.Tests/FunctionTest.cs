using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Xunit;

namespace headlines_lambda.Tests;

public class FunctionTest
{
  [Fact]
  public async Task TestFunction()
  {
    var inputData = "hello world";
    var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(inputData));
    // Invoke the lambda function and confirm the string was upper cased.
    var function = new Function();
    var context = new TestLambdaContext();
    await function.FunctionHandler(inputStream, context);
    // Invoke the lambda function and confirm the string was upper cased.
  }
}
