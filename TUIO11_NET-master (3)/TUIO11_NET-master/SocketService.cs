using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace MyApp
{
  class SocketService
  {
    public SocketService()
    {
    }
    public void getBluetoothDevicesAndUploadToDatabase()
    {
      try
      {

        String ip = "127.0.0.1";
        int port = 3000;
        IPAddress ipAddr = IPAddress.Parse(ip);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);

        Socket sender = new Socket(ipAddr.AddressFamily,
                  SocketType.Stream, ProtocolType.Tcp);

        try
        {

          sender.Connect(localEndPoint);
          Console.WriteLine($"Socket connected to {ip}:{port}");


          byte[] messageSent = Encoding.ASCII.GetBytes("Test Client<EOF>");
          int byteSent = sender.Send(messageSent);

          byte[] messageReceived = new byte[1024];

          int byteRecv = sender.Receive(messageReceived);

          String Response = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
          Console.WriteLine(Response);
          DeviceJson deviceJson = JsonSerializer.Deserialize<DeviceJson>(Response)!;
          Console.WriteLine(deviceJson.devices.Count);
          foreach (Device device in deviceJson.devices)
          {
            Console.WriteLine($"name:{device.name}, address: {device.address}");
          }
          sender.Shutdown(SocketShutdown.Both);
          sender.Close();
        }

        catch (ArgumentNullException ane)
        {

          Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
        }

        catch (SocketException se)
        {

          Console.WriteLine("SocketException : {0}", se.ToString());
        }

        catch (Exception e)
        {
          Console.WriteLine("Unexpected exception : {0}", e.ToString());
        }
      }

      catch (Exception e)
      {

        Console.WriteLine(e.ToString());
      }
    }

  }
}
