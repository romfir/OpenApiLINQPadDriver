//using LINQPad.Extensibility.DataContext;
//using Moq;
//using System.Xml.Linq;
//using FlaUI.UIA3;
//using OpenApiLINQPadDriver;
//using FluentAssertions;
//using FlaUI.Core.AutomationElements;
//using FlaUI.Core.Definitions;

//namespace OpenApiLINQPadDriverTests;
//public class UI
//{

//    [Fact]
//    public async Task Foo()
//    {
//        Action? close = null;
//        var connectionInfoMock = new Mock<IConnectionInfo>();

//        var xElement = XDocument.Parse(@$"    <DriverData>
//    <OpenApiDocumentUri>https://localhost/swagger/v1/swagger.json</OpenApiDocumentUri>
//    <ApiUri>https://localhost/</ApiUri>
//</DriverData>").Root!;

//        connectionInfoMock.Setup(s => s.DriverData).Returns(xElement);

//        var driverProperties = new OpenApiContextDriverProperties(connectionInfoMock.Object);

//        var thread = new Thread(() =>
//        {


//            var dialog = new ConnectionDialog(driverProperties);

//            close = () => dialog.Dispatcher.Invoke(dialog.Close);
//            var result = dialog.ShowDialog();

//            result.Should().BeTrue();
//        });

//        try
//        {
//            thread.SetApartmentState(ApartmentState.STA);
//            thread.Start();

//            var app = FlaUI.Core.Application.Attach(Environment.ProcessId);

//            using var automation = new UIA3Automation();

//            var window = app.GetMainWindow(automation);
//            window.Title.Should().Be("Open API Connection");

//            var okButton = window.FindFirstDescendant(cf => cf.ByText("OK"))?.AsButton();
//            var x = window.FindFirstDescendant(cf => cf.ByText("Open", PropertyConditionFlags.MatchSubstring)).AsLabel();


//            //var children = window.FindAllDescendants().ToList();

//            //children.Should().NotBeNull();
//            x.Should().NotBeNull();

//            x.Text.Should().Be("Open Api Json Uri (swagger.json): ");

//            var input = window.FindFirstDescendant(cf =>
//                    cf.ByAutomationId("OpenApiDocumentUri", PropertyConditionFlags.MatchSubstring))
//                .AsTextBox();

//            input.Should().NotBeNull();
//            input.Text.Should().Be("https://localhost/swagger/v1/swagger.json");


//            input.Enter("https://foobar/swagger/v1/swagger.json");

//            await Task.Delay(200);

//            okButton.Click();
//            close = null;

//            //result = result;
//        }
//        finally
//        {
//            close?.Invoke();
//            thread.Join();
//        }


//        driverProperties.OpenApiDocumentUri.Should().Be("https://foobar/swagger/v1/swagger.json");
//    }
//}
