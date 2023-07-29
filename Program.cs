using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Globalization;
using Client;
using Driver;
using Models;
using static Models.Stop;

namespace Testing;
class Program
{
  public static ArduinoDriver driver = new ArduinoDriver("COM3", 9600);
  public static TuBondiClient Client = new TuBondiClient();

  public static async Task CheckDeparting(IEnumerable<string> lines, Direction direction)
  {
    Console.Write("ASLdjka");
    foreach (string line in lines)
    {
      Console.WriteLine($"Data for {line}/{direction}");
      while (true)
      {
        List<CarDeparture>? departures = await Client.GetDeparting(line, direction);
        String hourMinute = DateTime.Now.Add(new TimeSpan(0, 10, 0)).ToString("HH:mm");
        foreach (CarDeparture car in departures)
        {
          Console.WriteLine($"{car.CarId}, {car.Line},  {car.RouteId},  {car.Direction}, {car.DepartureTime}");
        }
        if (departures.Find(e => TimeSpan.ParseExact(e.DepartureTime, @"hh\:mm", CultureInfo.InvariantCulture)
          < TimeSpan.ParseExact(hourMinute, @"hh\:mm", CultureInfo.InvariantCulture) && TimeSpan.ParseExact(e.DepartureTime, @"hh\:mm", CultureInfo.InvariantCulture)
          > TimeSpan.ParseExact(DateTime.Now.ToString("HH:mm"), @"hh\:mm", CultureInfo.InvariantCulture)
          ) != null)
        {
          driver.TurnOn();
        }
        else
        {
          driver.TurnOff();
        }
        Thread.Sleep(10000);
      }
    }
  }
  public static async Task Main()
  {
    var lineOption = new Option<IEnumerable<string>>(
      new[] { "-l", "--lines" },
      "The bus line wanted to check"
    )
    { AllowMultipleArgumentsPerToken = true, IsRequired = true };
    var directionOption = new Option<Direction>(
      new[] { "-d", "--direction" },
      "The direction of the Line"
    )
    { IsRequired = true };

    var checkArriving = new Command("check-arrivings", "Get arriving cars.") {
      lineOption,
      directionOption
    };

    // Define the command handler
    checkArriving.Handler = CommandHandler.Create<IEnumerable<string>, Direction>(
      async (IEnumerable<string> lines, Direction direction) => await CheckDeparting(lines, direction));

    // Build and invoke the parser
    var rootCommand = new RootCommand("A simple CLI application for Bus Tracking.")
    {
        checkArriving,
    };


    var builder = new CommandLineBuilder(rootCommand);
    builder.UseDefaults();

    // Invoke the command line parser
    var parser = builder.Build();
    var input = Console.ReadLine();

    await parser.InvokeAsync(input);
  }
}
