using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bebop2.Control {
  /**
   * <summary>Represents the connection between the controller and the drone.</summary>
   */
  [DebuggerDisplay("Drone @ {IP}")]
  public class Connection {
    public const int DEFAULT_C2D_PORT = 54321;
    public const int DEFAULT_D2C_PORT = 43210;
    public const int DEFAULT_DISCOVERY_PORT = 44444;
    public const int DEFAULT_STREAM_PORT = 55004;
    public const string DEFAULT_DRONE_IP = "192.168.42.1";
    public const int DEFAULT_TIMEOUT = 30;  // Timeout, in seconds.

    // How long to wait before resending a reliable command, in ms.
    private const int DEFAULT_ACK_RESEND_DELAY = 500;
    private const int DEFAULT_TX_DELAY = 10;  // Transmission delay, in ms.

    public string IP { get; private set; }

    /**
     * <summary>Device time, as last reported by the device.</summary>
     */
    public long DeviceTime {
      get {
        return device_time_;
      }
    }

    public Connection() {
      // Discover all events
      if (all_events_ == null) {
        var assembly = Assembly.GetCallingAssembly();
        var typ = from t in assembly.GetTypes()
                  let attributes = t.GetCustomAttributes(typeof(Events.EventInfo), true)
                  where attributes != null && attributes.Length == 1
                  select new Tuple<Type, Events.EventInfo>(t, (Events.EventInfo)attributes[0]);
        all_events_ = typ.ToList();
      }
    }

    public bool Connect(string ip, int command_port, int timeout = DEFAULT_TIMEOUT) {
      C2D_socket_ = new UdpClient(ip, command_port);
      D2C_socket_ = new UdpClient(DEFAULT_D2C_PORT);

      // Begin receiving data.
      D2C_socket_.BeginReceive(new AsyncCallback(OnDataReceived), null);
      C2D_socket_.Client.ReceiveTimeout = timeout;

      IP = ip;
      connection_open_ = true;
      return true;
    }

    public void Close() {
      // TODO(justin): Close everything out.
      pending_packets_.Clear();
      ack_pending_commands_.Clear();

      seq_num_ = 0;
      ack_seq_num_ = 0;
      device_time_ = 0;
      last_ping_time_ = 0;
      connection_open_ = false;
    }

    public bool SendCommand(Command cmd) {
      byte[] payload = cmd.GetData();
      byte[] packet = new byte[7 + 5 + (payload != null ? payload.Length : 0)];
      BinaryWriter bw = new BinaryWriter(new MemoryStream(packet));

      Protocol.FrameType frame_type = cmd.Reliable
                                          ? Protocol.FrameType.DATA_WITH_ACK
                                          : Protocol.FrameType.DATA;
      Protocol.FrameID frame_id = cmd.Reliable
                                      ? Protocol.FrameID.COMMAND_WITH_ACK
                                      : Protocol.FrameID.COMMAND;

      bw.Write((byte)frame_type);
      bw.Write((byte)frame_id);
      bw.Write(seq_num_++);
      bw.Write(packet.Length);

      // Command header
      bw.Write((byte)cmd.Project);
      bw.Write((byte)cmd.Class);
      bw.Write((ushort)cmd.ID);
      if (payload != null) {
        bw.Write(payload);
      }
      
      QueuePacket(packet, packet.Length);
      return true;
    }

    /**
     * <summary>
     *   Discovers if a drone is active at an IP address.
     *   You must discover a drone first before attempting to connect to it.
     * </summary>
     * <param name="ip">The IP address to ping</param>
     * <param param name="d2c_port">The port the drone will communicate with us on.</param>
     * <returns>A Protocol.DiscoveryResponse if the drone responded, otherwise null</returns>
     */
   public static async Task<Protocol.DiscoveryResponse?>
        DiscoverDroneAsync(string ip, int d2c_port = DEFAULT_D2C_PORT,
                           int stream_port = DEFAULT_STREAM_PORT) {
      try {
        TcpClient tcp = new TcpClient();
        await tcp.ConnectAsync(ip, DEFAULT_DISCOVERY_PORT);
        StreamWriter sw = new StreamWriter(tcp.GetStream());
        StreamReader sr = new StreamReader(tcp.GetStream());

        Protocol.DiscoveryPacket discovery;
        discovery.controller_name = Environment.MachineName;
        discovery.controller_type = "computer";
        discovery.d2c_port = d2c_port;
        discovery.arstream2_client_stream_port = 55004;
        discovery.arstream2_client_control_port = 55005;
        string json = JsonConvert.SerializeObject(discovery);
        sw.WriteLine(json);
        sw.Flush();

        var response = JsonConvert.DeserializeObject<Protocol.DiscoveryResponse>(await sr.ReadLineAsync());
        return response;
      } catch (SocketException) {
        return null;
      }
    }

    /**
     * <summary>
     *   Discovers if a drone is active at an IP address.
     *   You must discover a drone first before attempting to connect to it.
     * </summary>
     * <param name="ip">The IP address to ping</param>
     * <param param name="d2c_port">The port the drone will communicate with us on.</param>
     * <returns>A Protocol.DiscoveryResponse if the drone responded, otherwise null</returns>
     */
    public static Protocol.DiscoveryResponse?
        DiscoverDrone(string ip, int d2c_port = DEFAULT_D2C_PORT,
                      int stream_port = DEFAULT_STREAM_PORT) {
      try {
        using (TcpClient tcp = new TcpClient(ip, DEFAULT_DISCOVERY_PORT)) {
          StreamWriter sw = new StreamWriter(tcp.GetStream());
          StreamReader sr = new StreamReader(tcp.GetStream());

          Protocol.DiscoveryPacket discovery;
          discovery.controller_name = Environment.MachineName;
          discovery.controller_type = "computer";
          discovery.d2c_port = d2c_port;
          discovery.arstream2_client_stream_port = 55004;
          discovery.arstream2_client_control_port = 55005;
          string json = JsonConvert.SerializeObject(discovery);
          sw.WriteLine(json);
          sw.Flush();

          var response = JsonConvert.DeserializeObject<Protocol.DiscoveryResponse>(sr.ReadLine());
          return response;
        }
      } catch (SocketException) {
        return null;
      }
    }

    private byte[] BuildCommand(Command cmd) {
      byte[] payload = cmd.GetData();
      byte[] packet = new byte[7 + 5 + (payload != null ? payload.Length : 0)];
      BinaryWriter bw = new BinaryWriter(new MemoryStream(packet));

      Protocol.FrameType frame_type = cmd.Reliable
                                          ? Protocol.FrameType.DATA_WITH_ACK
                                          : Protocol.FrameType.DATA;
      Protocol.FrameID frame_id = cmd.Reliable
                                      ? Protocol.FrameID.COMMAND_WITH_ACK
                                      : Protocol.FrameID.COMMAND;

      bw.Write((byte)frame_type);
      bw.Write((byte)frame_id);
      bw.Write(seq_num_++);
      bw.Write(packet.Length);

      // Command header
      bw.Write((byte)cmd.Project);
      bw.Write((byte)cmd.Class);
      bw.Write((ushort)cmd.ID);
      if (payload != null) {
        bw.Write(payload);
      }

      return packet;
    }

    private byte[] BuildAck(byte seq_num, byte ack_seq) {
      byte[] packet = new byte[7 + 1];
      BinaryWriter writer = new BinaryWriter(new MemoryStream(packet));

      writer.Write((byte)Protocol.FrameType.ACK);
      writer.Write((byte)Protocol.FrameID.ACK_RESPONSE);
      writer.Write((byte)ack_seq);
      writer.Write((byte)seq_num);
      return packet;
    }

    private byte[] BuildPong(byte[] data, int offset, int length) {
      byte[] packet = new byte[7 + length];
      BinaryWriter writer = new BinaryWriter(new MemoryStream(packet));

      writer.Write((byte)Protocol.FrameType.DATA_LOW_LATENCY);
      writer.Write((byte)Protocol.FrameID.PONG);
      writer.Write((byte)0);
      writer.Write(length + 7);
      writer.Write(data, offset, length);
      return packet;
    }

    /**
     * <summary>Queues raw data to be sent to the remote device.</summary>
     */
    private void QueuePacket(byte[] data, int length) {
      // TODO(justin): Actually queue this.
      C2D_socket_.Send(data, length);
    }

    private void TXWorker() {
      // TODO: Send pending packets
      // Update and resend any unacknowledged reliable commands
      while (connection_open_) {
        // Send pending commmands
        lock (pending_packets_) {
          if (pending_packets_.Count == 0) {
            Thread.Sleep(DEFAULT_TX_DELAY);
            continue;
          }

          foreach (var c in pending_packets_) {
            // If reliable, make sure the target acknowledges it.
            if (c.Item2) {
              // ack_pending_commands_.Add()
            }

            C2D_socket_.Send(c.Item1, c.Item1.Count());
            Thread.Sleep(DEFAULT_TX_DELAY);
          }
        }

        // Check and resend any pending reliable commands that haven't been
        // acknowledged within the appropriate timeout
      }
    }

    private void RXWorker() {
      while (connection_open_) {
        IPEndPoint remote_addr = new IPEndPoint(IPAddress.Any, DEFAULT_D2C_PORT);
        byte[] data = D2C_socket_.Receive(ref remote_addr);

        // OnDataReceived(data)
      }
    }
    
    private void ProcessEventReceived(Stream stream) {
      BinaryReader reader = new BinaryReader(stream);
      byte command_project = reader.ReadByte();
      byte command_class = reader.ReadByte();
      ushort command_id = reader.ReadUInt16();

      var evts = from e in all_events_
                 let et = e.Item1
                 let ei = e.Item2
                 where ei.Project == command_project &&
                       ei.Class == command_class &&
                       ei.ID == command_id
                 select et;
      Debug.Assert(evts.Count() <= 1);

      if (evts.Count() == 1) {
        Events.Event evt = (Events.Event)Activator.CreateInstance(evts.ToArray()[0]);
        evt.SetData(reader);
        OnEventReceived(this, evt);
      }
    }

    private void OnCommandAcknowledged(byte cmd_seq) {
      lock (ack_pending_commands_) {
        for (int i = 0; i < ack_pending_commands_.Count; i++) {
          if (ack_pending_commands_[i].Item1 == cmd_seq) {
            ack_pending_commands_.RemoveAt(i);
            break;
          }
        }
      }
    }

    private void OnDataReceived(IAsyncResult ar) {
      // Receive data and disable callbacks.
      IPEndPoint remote_addr = new IPEndPoint(IPAddress.Any, DEFAULT_D2C_PORT);
      byte[] data = D2C_socket_.EndReceive(ar, ref remote_addr);

      Stream stream = new MemoryStream(data);
      BinaryReader reader = new BinaryReader(stream);
      Protocol.FrameType frame_type = (Protocol.FrameType)reader.ReadByte();
      Protocol.FrameID frame_id = (Protocol.FrameID)reader.ReadByte();
      byte seq_num = reader.ReadByte();
      uint size = reader.ReadUInt32();

      switch (frame_type) {
        case Protocol.FrameType.ACK:
          Debug.Assert(frame_id == Protocol.FrameID.COMMAND_ACK_RESPONSE);
          break;
        case Protocol.FrameType.DATA_WITH_ACK:
          var packet = BuildAck(seq_num, ack_seq_num_++);
          QueuePacket(packet, packet.Length);
          break;
      }

      switch (frame_id) {
        case Protocol.FrameID.PING:
          // Pong packet is a copy of the ping packet.
          var packet = BuildPong(data, 7, (int)size - 7);
          QueuePacket(packet, packet.Length);
          last_ping_time_ = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

          // Grab the current time from the drone.
          uint seconds = reader.ReadUInt32();
          uint nanoseconds = reader.ReadUInt32();
          device_time_ = seconds * 1000 + nanoseconds / 1000000;
          break;
        case Protocol.FrameID.PONG:
          // Sent by the drone to confirm that our ping packet was received.
          break;
        case Protocol.FrameID.NAVDATA:
        case Protocol.FrameID.EVENT:
          // Navigation data or events sent by the drone.
          ProcessEventReceived(stream);
          break;
      }

      // Re-enable receive callbacks.
      D2C_socket_.BeginReceive(new AsyncCallback(OnDataReceived), null);
    }

    // Public events
    public event EventHandler<Events.Event> OnEventReceived;

    // Packets pending send <data, reliable>
    private List<Tuple<byte[], bool>> pending_packets_ = new List<Tuple<byte[], bool>>();

    // Commands pending an acknowledgement from the remote device.
    // Stored as <seq_num, cmd, send_time>
    private List<Tuple<byte, byte[], long>> ack_pending_commands_ =
        new List<Tuple<byte, byte[], long>>();

    private bool connection_open_ = false;
    private byte seq_num_ = 0;
    private byte ack_seq_num_ = 0;
    private long device_time_ = 0; // Device time, in milliseconds.
    private long last_ping_time_ = 0;
    private UdpClient C2D_socket_; // Controller to Drone
    private UdpClient D2C_socket_; // Drone to Controller
    private static List<Tuple<Type, Events.EventInfo>> all_events_;
  }
}
