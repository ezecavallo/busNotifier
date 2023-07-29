using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Client;
using static Models.Stop;

namespace Models;

public enum Direction
{
  [EnumMember(Value = "I")]
  I = 'I',
  [EnumMember(Value = "V")]
  V = 'V'
}

public class TuBondiClient
{
  private readonly static TuBondiWrapper Wrapper = new TuBondiWrapper();
  public LinesDeserializer? Lines { get; set; } = null;

  public async Task InitLines()
  {
    if (Lines == null)
    {
      Lines = JsonSerializer.Deserialize<LinesDeserializer>(await Wrapper.GetLines());
    }
  }
  public async Task<List<CarDeparture>?> GetDeparting(string bus, Direction direction)
  {
    await InitLines();
    Line? line = Lines.SearchLine(bus);
    if (line == null)
    {
      throw new Exception($"Line {bus} was not found.");
    }
    Route lineRoute = line.Routes.Find(e => e.Direction == direction.ToString());
    Route? route = JsonSerializer.Deserialize<Route>(await Wrapper.GetTrace(lineRoute.RouteId, line.ClientId));
    string stopCode = route.Stops.First().StopId;
    Stop stop = JsonSerializer.Deserialize<Stop>(await Wrapper.GetComingArrives(stopCode));
    List<CarDeparture> departures = new List<CarDeparture>();
    foreach (CarDeparture departure in stop.Departures)
    {
      if (departure.Line == line.LineId && departure.Direction == direction.ToString())
        departures.Add(departure);
    }
    return departures;
  }

  public Task GetDeparting(IEnumerable<string> buses)
  {
    throw new Exception("Not implemented");
  }
}

public class Line
{
  [JsonPropertyName("linea_id")]
  public string LineId { get; set; }
  [JsonPropertyName("cliente")]
  public int ClientId { get; set; }
  [JsonPropertyName("linea_nombre")]
  public string Name { get; set; }
  [JsonPropertyName("rutas")]
  public List<Route> Routes { get; set; }
}

public class LinesDeserializer
{
  [JsonPropertyName("lineas")]
  public List<Line> Lines { get; set; }

  public Line? SearchLine(string bus)
  {
    if (Lines == null)
    {
      throw new Exception();
    }

    foreach (Line line in Lines)
    {
      Console.WriteLine(line.Name);
      if (line.Name == bus)
        return line;
    }
    return null;
  }

}

public class Car
{
  public string CarId { get; set; }
  [JsonPropertyName("linea_nombre")]
  public string Line { get; set; }
  [JsonPropertyName("ruta_id")]
  public string RouteId { get; set; }
  public string ItineraryId { get; set; }
  public double Lang { get; set; }
  public double Lat { get; set; }
  [JsonPropertyName("sentido")]
  public string Direction { get; set; }
  [JsonPropertyName("cliente")]
  public int Client { get; set; }
}

public class Arrives
{
  [JsonPropertyName("coches")]
  public List<Car> Cars { get; set; }
}

public class Stop
{
  [JsonPropertyName("codigo")]
  public string StopId { get; set; }
  [JsonPropertyName("parada_nombre")]
  public string Name { get; set; }
  [JsonPropertyName("lon")]
  public string Lang { get; set; }
  [JsonPropertyName("lat")]
  public string Lat { get; set; }
  public int Client { get; set; }
  [JsonPropertyName("proximos_arribos")]
  public List<CarDeparture> Departures { get; set; }

  public class CarDeparture
  {
    public string CarId { get; set; }
    [JsonPropertyName("linea_nombre")]
    public string Line { get; set; }
    [JsonPropertyName("ruta_id")]
    public string RouteId { get; set; }
    [JsonPropertyName("sentido")]
    public string Direction { get; set; }
    [JsonPropertyName("proximo")]
    public string Next { get; set; }
    [JsonPropertyName("hora_salida")]
    public string DepartureTime { get; set; }
    [JsonPropertyName("horaTeorica")]
    public string ArrivingTime { get; set; }
    [JsonPropertyName("demora")]
    public string Delay { get; set; }
  }
}


public class Route
{
  [JsonPropertyName("ruta_id")]
  public string RouteId { get; set; }
  public List<Car> Cars { get; set; }
  [JsonPropertyName("paradas")]
  public List<Stop> Stops { get; set; }
  [JsonPropertyName("traza")]
  public List<List<double>> Traces { get; set; }
  [JsonPropertyName("sentido")]
  public string Direction { get; set; }

}

// public class Route
// {
//   public TuBondiWrapper Client { get; set; }
//   public int RouteId { get; set; }
//   public List<Car> Cars { get; set; }
//   [JsonPropertyName("paradas")]
//   public List<Stop> Stops { get; set; }
//   [JsonPropertyName("traza")]
//   public List<List<double>> Traces { get; set; }
//   public Route(TuBondiWrapper client, int routeId)
//   {
//     Client = client;
//     RouteId = routeId;
//   }

//   public async Task<Route> GetRoute(int route, int client)
//   {
//     var responseObject = JsonSerializer.Deserialize<Route>(await Client.GetTrace(RouteId, 441));
//     Stops = responseObject.Stops;
//     Traces = responseObject.Traces;
//     return this;
//   }
//   public async Task<Route> GetCars()
//   {
//     Console.WriteLine(await Client.GetCarsPerRouter(RouteId));
//     Arrives responseObject = JsonSerializer.Deserialize<Arrives>(await Client.GetCarsPerRouter(RouteId));
//     Cars = responseObject.Cars;
//     return this;
//   }
// }
