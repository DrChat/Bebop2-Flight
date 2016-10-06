using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bebop2.FlightData {
  public class PUDFile {
    public class Detail {
      public string Name;
      public string Type;
      public int Size;

      public List<object> Data = new List<object>();
    }

    public List<Detail> Details = new List<Detail>();

    public void Deserialize(Stream input) {
      int b;
      var sb = new StringBuilder();
      while ((b = input.ReadByte()) > 0) {
        sb.Append((char)b);
      }

      var json = sb.ToString();
      var header = JsonConvert.DeserializeObject<dynamic>(json);

      var details_headers = header.details_headers;
      foreach (var detail_header in details_headers) {
        var name = detail_header.name;
        var type = detail_header.type;
        var size = detail_header.size;

        var detail = new Detail();
        detail.Name = name;
        detail.Type = type;
        detail.Size = size;

        switch (detail.Type) {
          case "integer":
            if (detail.Size != 1 && detail.Size != 2 && detail.Size != 4 && detail.Size != 8) {
              throw new InvalidDataException(String.Format("Unsupported sizeof(integer)={0}", detail.Size));
            }
            break;
          case "boolean":
            if (detail.Size != 1) {
              throw new InvalidDataException("sizeof(boolean) != 1");
            }
            break;
          case "float":
            if (detail.Size != 4) {
              throw new InvalidDataException("sizeof(float) != 4");
            }
            break;
          case "double":
            if (detail.Size != 8) {
              throw new InvalidDataException("sizeof(double) != 8");
            }
            break;
          default:
            throw new InvalidDataException(String.Format("Unsupported type {0}", detail.Type));
        }

        Details.Add(detail);
      }

      // Okay, now data is stored interleaved.
      // I.E. if details are "speed","location" then data is stored as speed,location,speed,location,...
      BinaryReader br = new BinaryReader(input);
      while (input.Position < input.Length) {
        try {
          foreach (var detail in Details) {
            switch (detail.Type) {
              case "integer":
                switch (detail.Size) {
                  case 1:
                    detail.Data.Add(br.ReadChar());
                    break;
                  case 2:
                    detail.Data.Add(br.ReadInt16());
                    break;
                  case 4:
                    detail.Data.Add(br.ReadInt32());
                    break;
                  case 8:
                    detail.Data.Add(br.ReadInt64());
                    break;
                }
                break;
              case "boolean":
                detail.Data.Add(br.ReadBoolean());
                break;
              case "double":
                detail.Data.Add(br.ReadDouble());
                break;
              case "float":
                detail.Data.Add(br.ReadSingle());
                break;
              default:
                break;
            }
          }
        } catch (EndOfStreamException) {
          throw new InvalidDataException("Unexpected end of file encountered!");
        }
      }
    }
  }
}
