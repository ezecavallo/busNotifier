using System.Drawing;
using System.IO.Ports;

namespace Driver;

class ArduinoDriver
{
  private static readonly int NumLeds = 8;
  private static readonly Color[] LedColors = new Color[NumLeds];
  public SerialPort? _serialPort { get; set; } = null;
  private static readonly string MagicWord = "Ada";
  private readonly byte[] SerialData = new byte[6 + NumLeds * 3];

  public ArduinoDriver(string portName, int baudRate)
  {
    _serialPort = new SerialPort();
    _serialPort.PortName = portName;
    _serialPort.BaudRate = baudRate;
    Console.Write("hi");
    OpenPort();
    Console.Write("hi2");
    SetSerialDataFrameHeader();
    for (var i = 0; i < NumLeds; i++)
    {
      LedColors[i] = Color.FromArgb(255, 0, 0);
    }
    Console.Write("hi3");
  }

  private void SetSerialDataFrameHeader()
  {
    // A special header / magic word is expected by the corresponding LED
    // streaming code running on the Arduino.
    SerialData[0] = Convert.ToByte(MagicWord[0]); // Magic word
    SerialData[1] = Convert.ToByte(MagicWord[1]);
    SerialData[2] = Convert.ToByte(MagicWord[2]);
    SerialData[3] = (byte)((NumLeds - 1) >> 8); // LED count high byte
    SerialData[4] = (byte)((NumLeds - 1) & 0xff); // LED count low byte
    SerialData[5] = (byte)(SerialData[3] ^ SerialData[4] ^ 0x55); // Checksum
  }

  private void SendSerialData()
  {
    var serialOffset = 6;
    foreach (var c in LedColors)
    {
      SerialData[serialOffset++] = c.R;
      SerialData[serialOffset++] = c.G;
      SerialData[serialOffset++] = c.B;
    }
    if (_serialPort != null && _serialPort.IsOpen)
    {
      _serialPort.Write(SerialData, 0, SerialData.Length);
    }
  }

  private void OpenPort()
  {
    try
    {
      if (_serialPort != null)
      {
        _serialPort.Open();
      }
    }
    catch (System.Exception)
    {
      Console.WriteLine("Unable to open COM port. Check it's not in use.");
    }
  }


  public void TurnOn()
  {
    if (_serialPort != null && _serialPort.IsOpen)
    {
      for (var i = 0; i < NumLeds; i++)
      {
        LedColors[i] = Color.FromArgb(255, 255, 255);
      }
      SendSerialData();
    }
  }
  public void TurnOff()
  {
    if (_serialPort != null && _serialPort.IsOpen)
    {
      for (var i = 0; i < NumLeds; i++)
      {
        LedColors[i] = Color.FromArgb(0, 0, 0);
        // LedColors[0].AdjustTemperature(AdjustedColorTemperature).Transition(PrevColors[i], Fade)
      }
      SendSerialData();
    }
  }
}