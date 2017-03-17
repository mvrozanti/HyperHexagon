using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HyperHexagon {

    public struct Wall {
        public int slot;
        public int distance;//max value = 
        public int height;
        public override string ToString() {
            return slot + "," + distance + "," + height;
        }
    }

    class SuperHexagon {

        [DllImport("D:\\GoogleDrive\\Programming\\C++\\LoadDLL\\Debug\\LoadDLL.dll")]
        public static extern void PressEscKey();

        [DllImport("D:\\GoogleDrive\\Programming\\C++\\LoadDLL\\Debug\\LoadDLL.dll")]
        public static extern void PressSpaceKey();

        public VAMemory vam;
        public static readonly IntPtr BASE_POINTER = (IntPtr)0x0018FEE4;
        public static Dictionary<string, int> offsets = new Dictionary<string, int> {
            {"slotCount", 0x1BC},
            {"wallCount", 0x2930},
            {"firstWall", 0x210},
            {"wasHit",  0x2964},
            {"envRotation", 0x2968},
            {"playerAngle", 0x2958},
            {"playerAngleAux", 0x2954},
            {"mouseDownLeft", 0x42858},
            {"mouseDownRight", 0x4285A},
            {"mouseDown", 0x42C45},
            {"mapAngle", 0x1AC},
            {"score", 0x2988},
            {"clockwiseMove",  0x40A62},
            {"counterClockwiseMove", 0x40A60}
        };

        public SuperHexagon() {
            vam = new VAMemory("Super Hexagon");
        }

        public List<Wall> getWalls() {
            List<Wall> wallList = new List<Wall>();
            int offset = offsets["firstWall"];
            int wallCount = getWallCount();
            for (int i = 0; i < wallCount; i++) {
                IntPtr address = (IntPtr)vam.ReadInt32(BASE_POINTER) + offset + (i * 0x14);
                Wall w = new Wall();
                w.slot = vam.ReadInt32(address + 0x10);
                w.distance = vam.ReadInt32(address + 0x14);
                w.height = vam.ReadInt32(address + 0x18);
                wallList.Add(w);
            }
            return wallList;
        }

        public Dictionary<int, Wall> getForeseeableWalls() {
            Dictionary<int, Wall> walls = new Dictionary<int, Wall>();
            for (int i = 0; i < getSlotCount(); i++) {
                int minDist = 999999;
                Wall chosen = new Wall();
                foreach (Wall w in getWalls()) {
                    if (w.distance < minDist && w.slot == i) {
                        minDist = w.distance;
                        chosen = w;
                    }
                }
                walls.Add(i, chosen);
            }
            return walls;
        }

        int maxDistance = 0;
        int maxHeight = 0;
        public void StartAvoid(bool verbose) {
            int maxWallCount = 0;
            while (true) {
                List<Wall> newWallList = getWalls();
                int slotCount = getSlotCount();
                if (verbose)
                    Console.Clear();
                if (maxWallCount < newWallList.Count) {
                    maxWallCount = newWallList.Count;
                }
                Dictionary<int, int> possibleSlots = new Dictionary<int, int>();
                foreach (Wall w in newWallList) {
                    int slot = w.slot;
                    int distance = w.distance;
                    int height = w.height;
                    if (slot < slotCount && w.height > 0 && w.distance < 1000000 && w.slot > -1) {
                        if (possibleSlots.ContainsKey(slot)) {
                            if (distance < possibleSlots[slot]) {
                                possibleSlots[slot] = distance;
                            }
                        } else {
                            possibleSlots[slot] = distance;
                        }
                    }
                    if(distance > this.maxDistance) {
                        this.maxDistance = distance;
                    }
                    if (height > this.maxHeight) {
                        this.maxHeight = height;
                    }
                }
                for (int i = 0; i < slotCount; i++) {
                    if (!possibleSlots.ContainsKey(i)) {
                        possibleSlots.Add(i, int.MaxValue);
                    }
                }
                Console.WriteLine("Player slot: " + getPlayerSlot());
                Console.WriteLine("Max height: " + maxHeight);
                Console.WriteLine("Max distance: " + this.maxDistance);
                foreach (KeyValuePair<int, Wall> kvp in getForeseeableWalls()) {
                    Console.WriteLine(kvp.Value);
                }
                int targetSlot = -1;
                int maxDistance = -1;
                foreach (KeyValuePair<int, int> kvp in possibleSlots) {
                    if (kvp.Value > maxDistance) {
                        maxDistance = kvp.Value;
                        targetSlot = kvp.Key;
                    }
                }
                if (!setPlayerSlot(targetSlot)) {
                    Console.Beep();
                    Console.Beep();
                    Console.Beep();
                    Console.Beep();
                    Console.Beep();
                    Console.Beep();
                }
            }
        }

        /**
         * movement types
         **/
        public readonly int CLOCKWISE = 0;
        public readonly int COUNTERCLOCKWISE = 1;
        public readonly int STOP = 2;

        public void setMovement(int type) {
            moveCounterClockwise(type == COUNTERCLOCKWISE);
            moveClockwise(type == CLOCKWISE);
        }

        private bool moveClockwise(bool startOrStop) {
            return vam.WriteInt32((IntPtr)vam.ReadInt32(
                        BASE_POINTER) + offsets["clockwiseMove"], startOrStop ? 1 : 0);
        }

        private bool moveCounterClockwise(bool startOrStop) {
            return vam.WriteInt32((IntPtr)vam.ReadInt32(
                        BASE_POINTER) + offsets["counterClockwiseMove"], startOrStop ? 1 : 0);
        }

        public bool setPlayerSlot(int slot) {
            int slotCount = getSlotCount();
            int angle = 360 / slotCount * (slot % slotCount) + (180 / slotCount);
            return vam.WriteInt32((IntPtr)vam.ReadInt32(
                        BASE_POINTER) + offsets["playerAngle"], angle) &&
            vam.WriteInt32((IntPtr)vam.ReadInt32(
                        BASE_POINTER) + offsets["playerAngleAux"], angle);
        }

        public int getSlotCount() {
            return vam.ReadInt32((IntPtr)
                    vam.ReadInt32(
                        BASE_POINTER) + offsets["slotCount"]);
        }

        public int getPlayerSlot() {
            int angle = getPlayerAngle();
            int slotCount = getSlotCount();
            return (int)Math.Round((double)angle / 360 * slotCount, 1);
        }

        public int getPlayerAngle() {
            return vam.ReadInt32((IntPtr)
                    vam.ReadInt32(
                        BASE_POINTER) + offsets["playerAngle"]);
        }

        public int getWallCount() {
            return vam.ReadInt32((IntPtr)
                    vam.ReadInt32(
                        BASE_POINTER) + offsets["wallCount"]);
        }

        public int isDead() {
            return vam.ReadInt32((IntPtr)
                    vam.ReadInt32(
                        BASE_POINTER) + offsets["wasHit"]);
        }

        public int getEnvironmentRotation() {
            return vam.ReadInt32((IntPtr)
                vam.ReadInt32(
                    BASE_POINTER) + offsets["envRotation"]);
        }

        public void resetState() {
            PressEscKey();
            Thread.Sleep(2000);
            PressSpaceKey();
            Thread.Sleep(200);
        }
    }
}
