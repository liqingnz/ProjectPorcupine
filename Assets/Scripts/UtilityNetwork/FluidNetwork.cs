﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectPorcupine.PowerNetwork
{
    public class FluidNetwork
    {
        private readonly HashSet<UtilityGrid> fluidGrids;
        private readonly float secondsToTick = 1.0f;
        private float secondsPassed;

        public FluidNetwork()
        {
            fluidGrids = new HashSet<UtilityGrid>();
        }

        public bool IsEmpty
        {
            get { return fluidGrids.Count == 0; }
        }

        public bool CanPlugIn(IPluggable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return fluidGrids.Any(grid => grid.CanPlugIn(connection));
        }

        public bool PlugIn(IPluggable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (IsEmpty || !fluidGrids.Any(grid => grid.CanPlugIn(connection)))
            {
                fluidGrids.Add(new UtilityGrid());
                UnityDebugger.Debugger.LogWarning("FluidNetwork", "Adding new Fluid UtilityGrid");
            }

            // TODO: Currently, this will create a "Universal" Fluid system... that is not ideal.
            // In theory at this point there should either be a grid that can be plugged in, or there should be new grid added... that can be plugged in.
            UtilityGrid fluidGrid = fluidGrids.FirstOrDefault(grid => grid.CanPlugIn(connection));
            return PlugIn(connection, fluidGrid);
        }

        public bool PlugIn(IPluggable connection, UtilityGrid grid)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (!fluidGrids.Contains(grid))
            {
                fluidGrids.Add(grid);
            }

            return grid != null && grid.PlugIn(connection);
        }

        public bool IsPluggedIn(IPluggable connection, out UtilityGrid grid)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (IsEmpty)
            {
                grid = null;
                return false;
            }

            grid = fluidGrids.First(fluidGrid => fluidGrid.IsPluggedIn(connection));
            return grid != null;
        }

        public void Unplug(IPluggable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            UtilityGrid grid;
            IsPluggedIn(connection, out grid);
            if (grid == null)
            {
                return;
            }

            Unplug(connection, grid);
        }

        public void RegisterGrid(UtilityGrid grid)
        {
            if (fluidGrids.Contains(grid))
            {
                return;
            }

            fluidGrids.Add(grid);
        }

        public void UnregisterGrid(UtilityGrid grid)
        {
            if (!fluidGrids.Contains(grid))
            {
                return;
            }

            fluidGrids.Remove(grid);
        }

        public int FindId(UtilityGrid grid)
        {
            return fluidGrids.ToList().IndexOf(grid);
        }

        public void Unplug(IPluggable connection, UtilityGrid grid)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }

            grid.Unplug(connection);
        }

        public bool HasPower(IPluggable connection)
        {
            UtilityGrid grid;
            IsPluggedIn(connection, out grid);
            return grid != null && grid.IsOperating;
        }

        public void Update(float deltaTime)
        {
            secondsPassed += deltaTime;
            if (secondsPassed < secondsToTick)
            {
                return;
            }

            secondsPassed = 0.0f;
            Tick();
        }

        public void RemoveGrid(UtilityGrid grid)
        {
            fluidGrids.Remove(grid);
        }

        private void Tick()
        {
            if (IsEmpty)
            {
                return;
            }

            foreach (UtilityGrid fluidGrid in fluidGrids)
            {
                fluidGrid.Tick();
            }
        }
    }
}
