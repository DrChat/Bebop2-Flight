using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bebop2.Control {
  public class Drone {
    public bool Connected { get; private set; }
    public string IP { get; private set; }

    public Drone() {
      connection_.OnEventReceived += OnEvent;
    }

    public bool Connect(string ip) {
      Protocol.DiscoveryResponse? response = Connection.DiscoverDrone(ip);
      if (response == null) {
        return false;
      }
      
      Connected = connection_.Connect(ip, response.Value.c2d_port);
      if (Connected) {
        IP = ip;
      }
      
      return true;
    }

    public async Task<bool> ConnectAsync(string ip) {
      Protocol.DiscoveryResponse? response = await Connection.DiscoverDroneAsync(ip);
      if (response == null) {
        return false;
      }
      
      Connected = connection_.Connect(ip, response.Value.c2d_port);
      if (Connected) {
        IP = ip;
      }
      
      return true;
    }

    public void Disconnect() {
      
    }

    public bool EnableVideoStream(bool enable) {
      return connection_.SendCommand(new Commands.MediaStreaming.CmdVideoEnable(enable));
    }

    public bool RequestAllStates() {
      return connection_.SendCommand(new Commands.CmdAllStates());
    }

    public bool SetDateTime(DateTime time) {
      bool success = connection_.SendCommand(new Commands.CmdSetDate(time));
      success = success && connection_.SendCommand(new Commands.CmdSetTime(time));
      return success;
    }

    public bool TakeOff() {
      return connection_.SendCommand(new Commands.Piloting.CmdTakeoff());
    }

    public bool Land() {
      return connection_.SendCommand(new Commands.Piloting.CmdLand());
    }

    public bool EmergencyLand() {
      return connection_.SendCommand(new Commands.Piloting.CmdEmergency());
    }

    /**
     * <summary>Moves the drone.</summary>
     * <param name="active">Active</param>
     * <param name="roll">Roll parameter (left/right)</param>
     * <param name="pitch">Pitch parameter (forwards/backwards)</param>
     * <param name="yaw">Yaw parameter (left/right)</param>
     * <param name="gaz">Gaz parameter (up/down)</param>
     * <param name="psi">PSI Parameter (magnetic north)</param>
     */
    public bool Move(bool active, sbyte roll, sbyte pitch, sbyte yaw, sbyte gaz, float psi = 0) {
      return connection_.SendCommand(new Commands.Piloting.CmdPCmd(active ? (byte)1 : (byte)0, roll, pitch, yaw, gaz, psi));
    }

    /**
     * <summary>Moves the camera.</summary>
     * <param name="tilt">Tilt of the camera in degrees.</param>
     * <param name="pan">Pan of the camera in degrees.</param>
     */
    public bool MoveCamera(sbyte tilt, sbyte pan) {
      return connection_.SendCommand(new Commands.Camera.CmdMoveCamera(tilt, pan));
    }

    private void OnEvent(object sender, Events.Event gevt) {
      if (gevt.Project == 0) {
        // Common events
        switch (gevt.Class) {
          // CommonState
          case 5:
            switch (gevt.ID) {
              // Battery
              case 1: {
                var evt = (Events.EvtBatteryChanged)gevt;
                OnBatteryChanged(this, evt.Battery);
              } break;
              // Wifi
              case 7: { 
                var evt = (Events.EvtWifiSignalChanged)gevt;
                OnWifiSignalChanged(this, evt.DBm);
              } break;
            }
            break;
        }
      } else if (gevt.Project == 1) {
        // Ardrone3 events
        switch (gevt.Class) {
          // PilotingState
          case 4:
            switch (gevt.ID) {
              // Speed
              case 5: {
                var evt = (Events.PilotingState.EvtSpeedChanged)gevt;
                OnSpeedChanged(this, new float[]{evt.SpeedX, evt.SpeedY, evt.SpeedZ});
              } break;
              // Altitude
              case 6: {
                var evt = (Events.PilotingState.EvtAltitudeChanged)gevt;
                OnAltitudeChanged(this, evt.Altitude);
              } break;
            }
            break;
        }
      }
    }

    // Events
    public event EventHandler<byte> OnBatteryChanged;
    public event EventHandler<short> OnWifiSignalChanged;
    public event EventHandler<float[]> OnSpeedChanged;
    public event EventHandler<double> OnAltitudeChanged;

    private Connection connection_ = new Connection(); // Connection to the drone.
  }
}
