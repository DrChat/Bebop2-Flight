using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XInput.Wrapper;

namespace DroneFlightTool {
  class Gamepad {
    // HACK: This is ugly.
    public X.Gamepad XGamepad {
      get {
        return gamepad_;
      }
    }

    public bool Connected {
      get {
        return gamepad_.IsConnected;
      }
    }

    public Gamepad() {

    }

    public bool Initialize() {
      if (!X.IsAvailable) {
        return false;
      }

      gamepad_ = X.Gamepad_1;
      return true;
    }

    public void Update() {
      gamepad_.Update();
    }

    // https://msdn.microsoft.com/en-us/library/windows/desktop/ee417001(v=vs.85).aspx
    public Vector<float> GetLStick() {
      float[] values = {gamepad_.LStick.X, gamepad_.LStick.Y, 0, 0};
      Vector<float> vec = new Vector<float>(values);
      float magnitude = (float)Math.Sqrt(vec[0] * vec[0] + vec[1] * vec[1]);
      Vector<float> norm = vec * (1/magnitude);

      float normalizedMagnitude = 0;

      if (magnitude > gamepad_.LStick_DeadZone) {
        // clip the magnitude at its expected maximum value
        if (magnitude > 32767) magnitude = 32767;

        // adjust magnitude relative to the end of the dead zone
        magnitude -= gamepad_.LStick_DeadZone;

        // optionally normalize the magnitude with respect to its expected range
        // giving a magnitude value of 0.0 to 1.0
        normalizedMagnitude = magnitude / (32767 - gamepad_.LStick_DeadZone);
      }

      return norm * normalizedMagnitude;
    }

    // https://msdn.microsoft.com/en-us/library/windows/desktop/ee417001(v=vs.85).aspx
    public Vector<float> GetRStick() {
      float[] values = {gamepad_.RStick.X, gamepad_.RStick.Y, 0, 0};
      Vector<float> vec = new Vector<float>(values);
      float magnitude = (float)Math.Sqrt(vec[0] * vec[0] + vec[1] * vec[1]);
      Vector<float> norm = vec * (1 / magnitude);

      float normalizedMagnitude = 0;

      if (magnitude > gamepad_.RStick_DeadZone) {
        // clip the magnitude at its expected maximum value
        if (magnitude > 32767) magnitude = 32767;

        // adjust magnitude relative to the end of the dead zone
        magnitude -= gamepad_.RStick_DeadZone;

        // optionally normalize the magnitude with respect to its expected
        // range
        // giving a magnitude value of 0.0 to 1.0
        normalizedMagnitude = magnitude / (32767 - gamepad_.RStick_DeadZone);
      }

      return norm * normalizedMagnitude;
    }

    public float GetLTrigger() {
      float magnitude = gamepad_.LTrigger;
      float normalizedMagnitude = 0;

      if (magnitude > gamepad_.LTrigger_Threshold) {
        if (magnitude > 255) magnitude = 255;

        magnitude -= gamepad_.LTrigger_Threshold;
        normalizedMagnitude = magnitude / (255 - gamepad_.LTrigger_Threshold);
      }

      return normalizedMagnitude;
    }

    public float GetRTrigger() {
      float magnitude = gamepad_.RTrigger;
      float normalizedMagnitude = 0;

      if (magnitude > gamepad_.RTrigger_Threshold) {
        if (magnitude > 255) magnitude = 255;

        magnitude -= gamepad_.RTrigger_Threshold;
        normalizedMagnitude = magnitude / (255 - gamepad_.RTrigger_Threshold);
      }

      return normalizedMagnitude;
    }

    private X.Gamepad gamepad_;
  }
}
