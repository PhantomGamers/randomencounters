﻿using System;
using RandomEncounters.Patch;
using Unity.Entities;

namespace RandomEncounters.Components
{
    public class Timer : IDisposable
    {
        private bool _enabled;
        private bool _isRunning;
        private DateTime _lastRunTime;
        private TimeSpan _delay;
        private Action<World> _action;
        private Func<TimeSpan> _delayAction;

        public void Start(Action<World> action, TimeSpan delay)
        {
            _delay = delay;
            _lastRunTime = DateTime.UtcNow - delay;
            _action = action;
            _enabled = true;
            ServerEvents.OnUpdate += Update;
        }

        public void Start(Action<World> action, Func<TimeSpan> delayAction)
        {
            _delayAction = delayAction;
            _delay = _delayAction.Invoke();
            _lastRunTime = DateTime.UtcNow;
            _action = action;
            _enabled = true;
            ServerEvents.OnUpdate += Update;
        }

        private void Update(World world)
        {
            if (!_enabled || _isRunning)
            {
                return;
            }

            if (_lastRunTime + _delay >= DateTime.UtcNow)
            {
                return;
            }

            _isRunning = true;
            try
            {
                Utils.Logger.LogDebug("Executing timer.");
                _action.Invoke(world);
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError(ex);
            }
            finally
            {
                if (_delayAction != null)
                {
                    _delay = _delayAction.Invoke();
                }
                _lastRunTime = DateTime.UtcNow;
                _isRunning = false;
            }
        }

        public void Stop()
        {
            ServerEvents.OnUpdate -= Update;
            _enabled = false;
        }

        public void Dispose()
        {
            if (_enabled)
            {
                Stop();
            }
        }
    }
}