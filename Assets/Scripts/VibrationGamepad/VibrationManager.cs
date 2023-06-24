namespace Cass.VibrationManager
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Universal vibration manager
    /// </summary>
    public static class VibrationManager
    {
        private static bool _isGamepad = false;

        private static CancellationTokenSource _tokenSource = default;

        private static float _leftPower = 0;

        private static float _rightPower = 0;

        static VibrationManager()
        {
            _tokenSource = new CancellationTokenSource();
            Application.wantsToQuit += OnApplicationQuit;
            InputSystem.onActionChange += OnDeviceChange;
            Vibration.Init();
            SetGamepadVibrationControl();
        }

        private static bool OnApplicationQuit()
        {
            _tokenSource.Cancel();
            return true;
        }

        private static void OnDeviceChange(object action, InputActionChange change)
        {

            if (change == InputActionChange.ActionPerformed)
            {
                var device = ((InputAction)action).activeControl.device;

                _isGamepad = device is Gamepad;
            }
        }
        /// <summary>
        /// Vibrate current gamepad / phone. Use multiply times for stronger effect. Side only for gamepad
        /// </summary>
        /// <param name="power"></param>
        /// <param name="type"></param>
        /// <param name="side"></param>
        public static async void Vibrate(VibrationPower power, VibrationType type, VibrationSide side = VibrationSide.Middle)
        {

#if UNITY_ANDROID

            if (_isGamepad)
            {
                switch (type)
                {
                    case VibrationType.Shot:
                        SetVibrationGamepad(side, power);
                        break;
                    case VibrationType.PingPong:
                        for (int i = 0; i < Enum.GetNames(typeof(VibrationSide)).Length * 2; i++)
                        {
                            SetVibrationGamepad((VibrationSide)i, power);
                            await Task.Delay((int)(Time.fixedDeltaTime * 2000));
                        }
                        for (int i = Enum.GetNames(typeof(VibrationSide)).Length * 2; i > 0; i--)
                        {
                            SetVibrationGamepad((VibrationSide)i, power);
                            await Task.Delay((int)(Time.fixedDeltaTime * 2000));
                        }
                        break;
                }

            }
            else
            {
                if (Vibration.HasVibrator())
                {
                    await VibrateAndroid(type, power);
                }
            }

#elif UNITY_IOS
    
        
#else
        
        if (_isGamepad)
        {
            switch (type)
            {
                case VibrationType.Shot:
                    SetVibrationGamepad(side, power);
                    break;
                case VibrationType.PingPong:
                    for (int i = 0; i < Enum.GetNames(typeof(VibrationSide)).Length * 2; i++)
                    {
                        SetVibrationGamepad((VibrationSide)i, power);
                        await Task.Delay((int)(Time.fixedDeltaTime * 2000));
                    }
                    for (int i = Enum.GetNames(typeof(VibrationSide)).Length * 2; i > 0; i--)
                    {
                        SetVibrationGamepad((VibrationSide)i, power);
                        await Task.Delay((int)(Time.fixedDeltaTime * 2000));
                    }
                    break;
            }
            
        }

#endif

        }

        private static async Task VibrateAndroid(VibrationType type, VibrationPower power)
        {
            Action currentVibrate = default;

            switch (power)
            {
                case VibrationPower.Easy:
                    currentVibrate = Vibration.VibratePop;
                    break;
                case VibrationPower.Medium:
                    currentVibrate = Vibration.Vibrate;
                    break;
                case VibrationPower.Strong:
                    currentVibrate = Vibration.VibratePeek;
                    break;
            }

            switch (type)
            {
                case VibrationType.Shot:
                    currentVibrate();
                    break;
                case VibrationType.PingPong:
                    for (int i = 0; i < Enum.GetNames(typeof(VibrationSide)).Length * 2; i++)
                    {
                        currentVibrate();
                        await Task.Delay((int)(Time.fixedDeltaTime * 2000));
                    }
                    break;
            }
        }

        private static void SetVibrationGamepad(VibrationSide side, VibrationPower power)
        {
            float vibPower = 0;

            switch (power)
            {
                case VibrationPower.Easy:
                    vibPower = 0.1f;
                    break;
                case VibrationPower.Medium:
                    vibPower = 0.5f;
                    break;
                case VibrationPower.Strong:
                    vibPower = 1f;
                    break;
            }

            switch (side)
            {
                case VibrationSide.Left:
                    _leftPower += vibPower;
                    break;
                case VibrationSide.LowLeft:
                    _leftPower += vibPower;
                    _rightPower += vibPower / 4;
                    break;
                case VibrationSide.Middle:
                    _leftPower += vibPower;
                    _rightPower += vibPower;
                    break;
                case VibrationSide.LowRight:
                    _leftPower += vibPower / 4;
                    _rightPower += vibPower;
                    break;
                case VibrationSide.Right:
                    _rightPower += vibPower;
                    break;
            }
        }

        private static async Task SetGamepadVibrationControl()
        {
            while (true)
            {
                if (_tokenSource.IsCancellationRequested || Gamepad.current == null)
                {
                    return;
                }

                float div = Time.fixedDeltaTime * 10;

                _leftPower *= div;
                _rightPower *= div;

                if (_leftPower <= 0)
                {
                    _leftPower = 0;
                }
                if (_rightPower <= 0)
                {
                    _rightPower = 0;
                }
                Gamepad.current.SetMotorSpeeds(_leftPower, _rightPower);
                await Task.Delay((int)(Time.fixedDeltaTime * 1000));
            }
        }

        #region enums

        public enum VibrationPower
        {
            Easy = 0,
            Medium = 1,
            Strong = 2
        }

        public enum VibrationType
        {
            Shot = 0,
            PingPong = 1
        }

        public enum VibrationSide
        {
            Left = 0,
            LowLeft = 1,
            Middle = 2,
            LowRight = 3,
            Right = 4
        }

        #endregion
    }
}
