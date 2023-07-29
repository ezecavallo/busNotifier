
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using McMaster.Extensions.CommandLineUtils;
using Models;

namespace Client;
public class TuBondiWrapper
{
  private string baseUrl = "https://micronauta4.dnsalias.net/usuario";
  private readonly HttpClient httpClient = new HttpClient();
  public string UserAgent { get; private set; } = "";
  public string Cookie { get; private set; }


  private async Task<HttpRequestMessage> CreateRequestMessage(HttpMethod method, [StringSyntax("Uri")] string requestUri)
  {
    HttpRequestMessage requestMessage = new HttpRequestMessage(method, requestUri);
    // If not Cookie => Exception
    if (string.IsNullOrEmpty(Cookie))
    {
      Cookie = await GetCookie();
    }
    requestMessage.Headers.Add("Cookie", Cookie);
    requestMessage.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
    return requestMessage;
  }


  private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage)
  {
    IEnumerable<string> values;
    bool hasCookie = requestMessage.Headers.TryGetValues("Cookie", out values);
    if (!hasCookie)
    {
      // Throw exception
    }
    HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
    response.EnsureSuccessStatusCode();
    return response;
  }

  private async Task<string> GetCookie()
  {
    // Define RequestMessage
    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, baseUrl + "/urbano2.php?conf=cbaciudad");
    requestMessage.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

    // Send request
    HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
    response.EnsureSuccessStatusCode();

    // Get Cookie
    IEnumerable<string> values;
    bool hasCookie = response.Headers.TryGetValues("Set-Cookie", out values);
    if (!hasCookie)
    {
      // Throw exception
    }
    return values.First().Split(";").First();
  }

  public async Task<string> GetLines()
  {
    // Define RequestMessage
    HttpRequestMessage requestMessage = await CreateRequestMessage(
      HttpMethod.Post, baseUrl + "/urbano2_cmd.php?cmd=lineasyrutas"
    );
    Dictionary<string, string> data = new Dictionary<string, string>();
    data.Add("conf", "cbaciudad");
    requestMessage.Content = new FormUrlEncodedContent(data);

    // Send Request
    HttpResponseMessage response = await SendAsync(requestMessage);

    return await response.Content.ReadAsStringAsync();
  }

  public async Task<string> GetCarsPerRouter(int route)
  {
    // Define RequestMessage
    HttpRequestMessage requestMessage = await CreateRequestMessage(
      HttpMethod.Post, baseUrl + "/urbano2_cmd.php?cmd=consultacocheporruta"
    );
    Dictionary<string, string> data = new Dictionary<string, string>();
    data.Add("cliente", "411");
    data.Add("ruta", route.ToString());
    data.Add("conf", "cbaciudad");
    requestMessage.Content = new FormUrlEncodedContent(data);

    // Send Request
    HttpResponseMessage response = await SendAsync(requestMessage);

    return await response.Content.ReadAsStringAsync();
  }

  public async Task<string> GetTrace(string route, int client)
  {
    // Define RequestMessage
    HttpRequestMessage requestMessage = await CreateRequestMessage(
      HttpMethod.Post, baseUrl + "/urbano2_cmd.php/?cmd=seleccionatraza"
    );
    Dictionary<string, string> data = new Dictionary<string, string>(){
        {"cliente_id", client.ToString()},
        {"ruta", route},
        {"conf", "cbaciudad"},
      };
    requestMessage.Content = new FormUrlEncodedContent(data);

    // Send Request
    HttpResponseMessage response = await SendAsync(requestMessage);

    return await response.Content.ReadAsStringAsync();
  }

  public async Task<string> GetComingArrives(string stopCode)
  {
    // Define RequestMessage
    HttpRequestMessage requestMessage = await CreateRequestMessage(
      HttpMethod.Post, baseUrl + "/urbano2_cmd.php"
    );
    Dictionary<string, string> data = new Dictionary<string, string>(){
        {"cmd", "proximos_arribos"},
        {"codigo", stopCode},
        {"conf", "cbaciudad"},
      };
    requestMessage.Content = new FormUrlEncodedContent(data);

    // Send Request
    HttpResponseMessage response = await SendAsync(requestMessage);

    return await response.Content.ReadAsStringAsync();
  }
}
