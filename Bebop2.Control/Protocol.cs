using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bebop2.Control {
  public static class Protocol {
    public struct FrameHeader {
      public byte frame_type;
      public byte frame_id;
      public byte seq_num;
      public uint size;
    }

    public struct DiscoveryPacket {
      public string controller_type;
      public string controller_name;
      public int d2c_port;
      public int arstream2_client_stream_port;
      public int arstream2_client_control_port;
    }

    public struct DiscoveryResponse {
      /*
       * {
       *   "status": 0,
       *   "c2d_port": 54321,
       *   "arstream_fragment_size": 65000,
       *   "arstream_fragment_maximum_number": 4,
       *   "arstream_max_ack_interval": -1,
       *   "c2d_update_port": 51,
       *   "c2d_user_port": 21,
       *   "arstream2_server_stream_port": 5004,
       *   "arstream2_server_control_port": 5005,
       *   "arstream2_max_packet_size": 1500,
       *   "arstream2_max_latency": 0,
       *   "arstream2_max_network_latency": 200,
       *   "arstream2_max_bitrate": -1,
       *   "arstream2_parameter_sets": "TBD"
       * }
       */

      public int status;
      public int c2d_port;
      public int arstream_fragment_size;
      public int arstream_fragment_maximum_number;
      public int arstream_max_ack_interval;
      public int c2d_update_port;
      public int c2d_user_port;
      public int arstream2_server_stream_port;
      public int arstream2_server_control_port;
      public int arstream2_max_packet_size;
      public int arstream2_max_latency;
      public int arstream2_max_network_latency;
      public int arstream2_max_bitrate;
      public string arstream2_parameter_sets;
    }

    public enum FrameType {
      ACK = 1,
      DATA = 2,
      DATA_LOW_LATENCY = 3,
      DATA_WITH_ACK = 4,
    }

    public enum FrameID {
      PING = 0,
      PONG = 1,
      COMMAND = 0xA,
      COMMAND_WITH_ACK = 0xB,
      EMERGENCY = 0xC,
      VIDEO_ACK_RESPONSE = 0xD,
      VIDEO_DATA = 0x7D,
      EVENT = 0x7E,
      NAVDATA = 0x7F,
      COMMAND_ACK_RESPONSE = 0x8B,
      ACK_RESPONSE = 0xFE,
    }
  }
}
