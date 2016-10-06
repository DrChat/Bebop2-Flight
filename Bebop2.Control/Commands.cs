using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bebop2.Control {
  [AttributeUsage(AttributeTargets.Class)]
  public class CommandInfo : Attribute {
    public int Project { get; private set; }
    public int Class { get; private set; }
    public int ID { get; private set; }
    public bool Reliable { get; private set; }

    public CommandInfo(int proj, int cls, int id, bool reliable = false) {
      Project = proj;
      Class = cls;
      ID = id;
      Reliable = reliable;
    }
  }

  public abstract class Command {
    public int Project {
      get {
        CommandInfo attrib = (CommandInfo)Attribute.GetCustomAttribute(
            this.GetType(), typeof(CommandInfo));
        return attrib.Project;
      }
    }
    public int Class {
      get {
        CommandInfo attrib = (CommandInfo)Attribute.GetCustomAttribute(
            this.GetType(), typeof(CommandInfo));
        return attrib.Class;
      }
    }
    public int ID {
      get {
        CommandInfo attrib = (CommandInfo)Attribute.GetCustomAttribute(
            this.GetType(), typeof(CommandInfo));
        return attrib.ID;
      }
    }
    public bool Reliable {
      get {
        CommandInfo attrib = (CommandInfo)Attribute.GetCustomAttribute(
            this.GetType(), typeof(CommandInfo));
        return attrib.Reliable;
      }
    }

    public abstract byte[] GetData();
    public virtual void SetData(byte[] data, int offset) {}
  }

  namespace Commands {
    // ARCOMMANDS_ID_PROJECT_COMMON
    // ARCOMMANDS_ID_COMMON_CLASS_COMMON
    // ARCOMMANDS_ID_COMMON_COMMON_CMD_ALLSTATES
    [CommandInfo(0, 4, 0)]
    public class CmdAllStates : Command {
      public CmdAllStates() {}

      public override byte[] GetData() {
        return null;
      }
    }

    // ARCOMMANDS_ID_PROJECT_COMMON
    // ARCOMMANDS_ID_COMMON_CLASS_COMMON
    // ARCOMMANDS_ID_COMMON_COMMON_CMD_CURRENTTIME
    [CommandInfo(0, 4, 1)]
    public class CmdSetDate : Command {
      public DateTime date_;
      public CmdSetDate(DateTime date) {
        date_ = date;
      }

      public override byte[] GetData() {
        string encoded = date_.ToString("yyyy-MM-dd") + Char.MinValue;
        return ASCIIEncoding.ASCII.GetBytes(encoded);
      }
    }

    // ARCOMMANDS_ID_PROJECT_COMMON
    // ARCOMMANDS_ID_COMMON_CLASS_COMMON
    // ARCOMMANDS_ID_COMMON_COMMON_CMD_CURRENTTIME
    [CommandInfo(0, 4, 2)]
    public class CmdSetTime : Command {
      public DateTime time_;
      public CmdSetTime(DateTime time) {
        time_ = time;
      }

      public override byte[] GetData() {
        string encoded = time_.ToString("THH:mm:ssZ") + Char.MinValue;
        return ASCIIEncoding.ASCII.GetBytes(encoded);
      }
    }

    namespace MediaStreaming {
      // ARCOMMANDS_ID_PROJECT_ARDRONE3
      // ARCOMMANDS_ID_ARDRONE3_CLASS_MEDIASTREAMING
      // ARCOMMANDS_ID_ARDRONE3_MEDIASTREAMING_CMD_VIDEOENABLE
      [CommandInfo(1, 21, 0, true)]
      public class CmdVideoEnable : Command {
        public bool enable_;
        public CmdVideoEnable(bool enable) {
          enable_ = enable;
        }

        public override byte[] GetData() {
          byte[] data = new byte[1];
          data[0] = enable_ ? (byte)1 : (byte)0;
          return data;
        }
      }
    }

    namespace MediaStreamingState {
      // ARCOMMANDS_ID_PROJECT_ARDRONE3
      // ARCOMMANDS_ID_ARDRONE3_CLASS_MEDIASTREAMINGSTATE
      // ARCOMMANDS_ID_ARDRONE3_MEDIASTREAMINGSTATE_CMD_VIDEOENABLECHANGED
      [CommandInfo(1, 22, 0, true)]
      public class CmdVideoStreamingState : Command {
        public uint enabled_;
        public CmdVideoStreamingState(uint enabled) {
          enabled_ = enabled;
        }

        public override byte[] GetData() {
          byte[] data = new byte[4];
          BinaryWriter bw = new BinaryWriter(new MemoryStream(data));

          bw.Write(enabled_);
          return data;
        }

        public override void SetData(byte[] data, int offset) {
          Stream s = new MemoryStream(data);
          BinaryReader br = new BinaryReader(s);
          s.Position = offset;
          
          enabled_ = br.ReadUInt32();
        }
      }
    }

    namespace SettingsState {
      // ARCOMMANDS_ID_PROJECT_COMMON
      // ARCOMMANDS_ID_COMMON_CLASS_SETTINGSSTATE
      // ARCOMMANDS_ID_COMMON_SETTINGSSTATE_CMD_ALLSETTINGSCHANGED 
      [CommandInfo(0, 3, 1, true)]
      public class CmdAllSettingsChanged : Command {
        public CmdAllSettingsChanged() { }

        public override byte[] GetData() {
          return null;
        }
      }
    }

    namespace SpeedSettings {

    }

    namespace Piloting {
      // ARCOMMANDS_ID_PROJECT_ARDRONE3
      // ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING
      // ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_TAKEOFF
      [CommandInfo(1, 0, 1, true)]
      public class CmdTakeoff : Command {
        public CmdTakeoff() { }

        public override byte[] GetData() {
          return null;
        }
      }

      // ARCOMMANDS_ID_PROJECT_ARDRONE3
      // ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING
      // ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_PCMD 
      [CommandInfo(1, 0, 2, false)]
      public class CmdPCmd : Command {
        public byte flag_;
        public sbyte roll_;
        public sbyte pitch_;
        public sbyte yaw_;
        public sbyte gaz_;
        public float psi_;

        public CmdPCmd(byte flag, sbyte roll, sbyte pitch, sbyte yaw, sbyte gaz, float psi) {
          flag_ = flag;
          roll_ = roll;
          pitch_ = pitch;
          yaw_ = yaw;
          gaz_ = gaz;
          psi_ = psi;
        }

        public override byte[] GetData() {
          byte[] data = new byte[9];
          BinaryWriter bw = new BinaryWriter(new MemoryStream(data));
          bw.Write(flag_);
          bw.Write(roll_);
          bw.Write(pitch_);
          bw.Write(yaw_);
          bw.Write(gaz_);
          bw.Write(psi_);

          return data;
        }
      }


      // ARCOMMANDS_ID_PROJECT_ARDRONE3
      // ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING
      // ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_LANDING
      [CommandInfo(1, 0, 3, true)]
      public class CmdLand : Command {
        public CmdLand() { }

        public override byte[] GetData() {
          return null;
        }
      }

      // ARCOMMANDS_ID_PROJECT_ARDRONE3
      // ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING
      // ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_EMERGENCY
      [CommandInfo(1, 0, 4, true)]
      public class CmdEmergency : Command {
        public CmdEmergency() { }

        public override byte[] GetData() {
          return null;
        }
      }
    }

    namespace Camera {
      // ARCOMMANDS_ID_PROJECT_ARDRONE3
      // ARCOMMANDS_ID_ARDRONE3_CLASS_CAMERA
      // ARCOMMANDS_ID_ARDRONE3_CAMERA_CMD_ORIENTATION
      [CommandInfo(1, 1, 0, true)]
      public class CmdMoveCamera : Command {
        public sbyte tilt_;
        public sbyte pan_;

        public CmdMoveCamera(sbyte tilt, sbyte pan) {
          tilt_ = tilt;
          pan_ = pan;
        }

        public override byte[] GetData() {
          byte[] data = new byte[2];
          BinaryWriter bw = new BinaryWriter(new MemoryStream(data));
          bw.Write(tilt_);
          bw.Write(pan_);

          return data;
        }
      }
    }
  }
}
