using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bebop2.Control;
using Bebop2.FlightData;
using System.IO;
using System.Threading;
using System.Windows.Input;
using System.Numerics;
using System.Windows.Threading;
using System.Diagnostics;

namespace DroneFlightTool {
  public partial class MainForm : Form {
    public MainForm() {
      InitializeComponent();
      gamepad_.Initialize();
      drone_.OnBatteryChanged += (s, b) => {
        Invoke((MethodInvoker)delegate {
          battery_.Text = "Battery: " + b;

          if (b < 20) {
            battery_.BackColor = Color.Red;
          } else {
            battery_.BackColor = default(Color);
          }
        });
      };
      drone_.OnWifiSignalChanged += (s, dbm) => {
        Invoke(
            (MethodInvoker)delegate { wifi_.Text = "Wi-Fi: " + dbm + " dBm"; });
      };

      drone_.OnPositionChanged += (s, p) => {
        Invoke((MethodInvoker)delegate {
          position_.Text = "Position: " + p[0].ToString("0.000000") + " " +
                           p[1].ToString("0.000000") + " " +
                           p[2].ToString("0.00");
        });
      };
      drone_.OnPositionChanged += (s, p) => {
        Invoke((MethodInvoker)delegate {
          position_.Text = "Position: " + p[0].ToString("0.0000000") + " " + p[1].ToString("0.000000") + " " + p[2].ToString("0.0000");
        });
      };
      drone_.OnSpeedChanged += (s, v) => {
        Invoke((MethodInvoker)delegate {
          speed_.Text = "Speed: " + v[0].ToString("0.00") + " " +
                        v[1].ToString("0.00") + " " + v[2].ToString("0.00");
        });
      };
      drone_.OnAltitudeChanged += (s, a) => {
        Invoke((MethodInvoker)delegate {
          altitude_.Text = "Altitude: " + a.ToString("0.00");
        });
      };
    }

    private void OnConnected() {
      drone_.EnableVideoStream(true);
      drone_.RequestAllStates();
      // drone_.SetDateTime(DateTime.UtcNow);

      input_driver_thread_ = new Thread(InputDriverThread);
      input_driver_thread_.SetApartmentState(ApartmentState.STA);
      driver_running_ = true;

      input_driver_thread_.Start();
      control_panel_.Visible = true;
    }

    private void OnDisconnect() {
      driver_running_ = false;
      input_driver_thread_.Join();
      control_panel_.Visible = false;
    }

    private void MainForm_KeyPress(object sender, KeyPressEventArgs e) {
      if (drone_.Connected) {
        // TODO(justin): Use this instead of the input driver thread
      }
    }

    private void MainForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {

    }

    private void MainForm_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {

    }

    private async void connect_btn__Click(object sender, EventArgs e) {
      if (drone_.Connected) {
        // TODO(justin): Disconnect
        // OnDisconnect();
      } else {
        status_.Text = "Connecting";
        if (await drone_.ConnectAsync("192.168.42.1")) {
          status_.Text = "Connected";
          connect_btn_.Text = "Disconnect";
          OnConnected();
        } else {
          status_.Text = "Failed to connect";
        }
      }
    }

    private void InputDriverThread() {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      long emergency_pressed = 0;
      while (driver_running_) {
        stopwatch.Stop();
        double dt = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;

        sbyte roll = 0;
        sbyte pitch = 0;
        sbyte yaw = 0;
        sbyte gaz = 0;

        gamepad_.Update();
        if (!gamepad_.Connected) {
          // Only run keyboard input if the main form is in focus.
          if (Focused) {
            if (Keyboard.IsKeyDown(Key.T)) {
              drone_.TakeOff();
              continue;
            } else if (Keyboard.IsKeyDown(Key.L)) {
              drone_.Land();
              continue;
            } else if (Keyboard.IsKeyDown(Key.E)) {
              drone_.EmergencyLand();
              continue;
            }

            if (Keyboard.IsKeyDown(Key.W)) {
              // FORWARD
              pitch += 100;
            }
            if (Keyboard.IsKeyDown(Key.S)) {
              // BACKWARDS
              pitch -= 100;
            }
            if (Keyboard.IsKeyDown(Key.A)) {
              // GOING LEFT
              roll -= 100;
            }
            if (Keyboard.IsKeyDown(Key.D)) {
              // GOING RIGHT
              roll += 100;
            }
            if (Keyboard.IsKeyDown(Key.Up)) {
              gaz += 100;
            }
            if (Keyboard.IsKeyDown(Key.Down)) {
              gaz -= 100;
            }
            if (Keyboard.IsKeyDown(Key.Left)) {
              yaw -= 100;
            }
            if (Keyboard.IsKeyDown(Key.Right)) {
              yaw += 100;
            }
          }
        } else {
          // Gamepad input.
          if (gamepad_.XGamepad.Start_down) {
            drone_.TakeOff();
            continue;
          } else if (gamepad_.XGamepad.Back_down) {
            drone_.Land();
            continue;
          } else if (gamepad_.XGamepad.B_down) {
            // Delay an emergency landing by 2000 ms (2s)
            if (emergency_pressed == 0) {
              emergency_pressed = stopwatch.ElapsedMilliseconds;
            } else {
              long emergency_elapsed = stopwatch.ElapsedMilliseconds - emergency_pressed;
              if (emergency_elapsed > 2000) {
                drone_.EmergencyLand();
                emergency_pressed = 0;
              }
            }
            continue;
          } else {
            emergency_pressed = 0;
          }

          Vector2 LStick = gamepad_.GetLStick();
          Vector2 RStick = gamepad_.GetRStick();
          float LTrigger = gamepad_.GetLTrigger();
          float RTrigger = gamepad_.GetRTrigger();

          // LStick = movement
          // RStick = turning
          // Triggers = vertical movement
          // Cube each for fine-tune control
          LStick = LStick * LStick * LStick;
          RStick = RStick * RStick * RStick;
          LTrigger = LTrigger * LTrigger * LTrigger;
          RTrigger = RTrigger * RTrigger * RTrigger;

          LStick *= 100;
          RStick *= 100;

          LTrigger *= -100;
          RTrigger *= 100;

          roll = (sbyte)LStick.X;
          pitch = (sbyte)LStick.Y;
          yaw = (sbyte)RStick.X;
          gaz = (sbyte)(LTrigger + RTrigger);
        }

        stopwatch.Start();
        drone_.Move(true, roll, pitch, yaw, gaz);
        Thread.Sleep(20);
      }
    }

    private Thread input_driver_thread_;
    private volatile bool driver_running_ = false;
    private Drone drone_ = new Drone();
    private Gamepad gamepad_ = new Gamepad();
  }
}
