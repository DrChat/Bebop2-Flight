using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bebop2.Control {
  namespace Events {
    [AttributeUsage(AttributeTargets.Class)]
    public class EventInfo : Attribute {
      public int Project { get; private set; }
      public int Class { get; private set; }
      public int ID { get; private set; }

      public EventInfo(int proj, int cls, int id) {
        Project = proj;
        Class = cls;
        ID = id;
      }
    }

    public abstract class Event {
      public int Project {
        get {
          EventInfo attrib = (EventInfo)Attribute.GetCustomAttribute(
              this.GetType(), typeof(EventInfo));
          return attrib.Project;
        }
      }
      public int Class {
        get {
          EventInfo attrib = (EventInfo)Attribute.GetCustomAttribute(
              this.GetType(), typeof(EventInfo));
          return attrib.Class;
        }
      }
      public int ID {
        get {
          EventInfo attrib = (EventInfo)Attribute.GetCustomAttribute(
              this.GetType(), typeof(EventInfo));
          return attrib.ID;
        }
      }

      public virtual byte[] GetData() { return null; }
      public abstract void SetData(BinaryReader reader);
    }

    // ARCOMMANDS_ID_PROJECT_COMMON
    // ARCOMMANDS_ID_COMMON_CLASS_COMMONSTATE
    // ARCOMMANDS_ID_COMMON_COMMONSTATE_CMD_ALLSTATESCHANGED
    [EventInfo(0, 5, 0)]
    public class EvtAllStatesChanged : Event {
      public override void SetData(BinaryReader reader) { }
    }

    // ARCOMMANDS_ID_PROJECT_COMMON
    // ARCOMMANDS_ID_COMMON_CLASS_COMMONSTATE
    // ARCOMMANDS_ID_COMMON_COMMONSTATE_CMD_BATTERYSTATECHANGED
    [EventInfo(0, 5, 1)]
    public class EvtBatteryChanged : Event {
      public byte Battery { get; private set; }

      public override void SetData(BinaryReader reader) {
        Battery = reader.ReadByte();
      }
    }

    // ARCOMMANDS_ID_PROJECT_COMMON
    // ARCOMMANDS_ID_COMMON_CLASS_COMMONSTATE
    // ARCOMMANDS_ID_COMMON_COMMONSTATE_CMD_WIFISIGNALCHANGED
    [EventInfo(0, 5, 7)]
    public class EvtWifiSignalChanged : Event {
      public short DBm { get; private set; }

      public override void SetData(BinaryReader reader) {
        DBm = reader.ReadInt16();
      }
    }

    namespace PilotingState {
      // ARCOMMANDS_ID_PROJECT_ARDRONE3
      // ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTINGSTATE
      // ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_POSITIONCHANGED
      [EventInfo(1, 4, 4)]
      public class EvtPositionChanged : Event {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double Altitude { get; private set; }

        public override void SetData(BinaryReader reader) {
          Latitude = reader.ReadDouble();
          Longitude = reader.ReadDouble();
          Altitude = reader.ReadDouble();
        }
      }

      // ARCOMMANDS_ID_PROJECT_ARDRONE3
      // ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTINGSTATE
      // ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_SPEEDCHANGED
      [EventInfo(1, 4, 5)]
      public class EvtSpeedChanged : Event {
        public float SpeedX { get; private set; }
        public float SpeedY { get; private set; }
        public float SpeedZ { get; private set; }

        public override void SetData(BinaryReader reader) {
          SpeedX = reader.ReadSingle();
          SpeedY = reader.ReadSingle();
          SpeedZ = reader.ReadSingle();
        }
      }

      // ARCOMMANDS_ID_PROJECT_ARDRONE3
      // ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTINGSTATE
      // ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_ATTITUDECHANGED
      [EventInfo(1, 4, 6)]
      public class EvtAltitudeChanged : Event {
        public double Altitude { get; private set; }

        public override void SetData(BinaryReader reader) {
          Altitude = reader.ReadDouble();
        }
      }
    }

    namespace SettingsState {
      // ARCOMMANDS_ID_PROJECT_COMMON
      // ARCOMMANDS_ID_COMMON_CLASS_SETTINGSSTATE
      // ARCOMMANDS_ID_COMMON_SETTINGSSTATE_CMD_ALLSETTINGSCHANGED
      [EventInfo(0, 3, 0)]
      public class EvtAllSettingsChanged : Event {
        public override void SetData(BinaryReader reader) { throw new NotImplementedException(); }
      }
    }
  }
}
