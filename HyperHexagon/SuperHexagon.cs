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
        public int distance;
        public int height;
        public override string ToString() {
            return "Wall{" + slot + ", " + distance + ", " + height + '}';
        }
    }


    class SuperHexagon {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern uint ResumeThread(IntPtr hThread);
        public static VAMemory vam;
        public static bool wasHit = false;
        public static readonly IntPtr BASE_POINTER = (IntPtr)0x0018FEE4;
        public static Dictionary<string, int> offsets = new Dictionary<string, int> {
            {"slotCount", 0x1BC},
            {"wallCount", 0x2930},
            {"firstWall", 0x210},
            {"wasHit",  0x2964},
            {"deadBool", 0x2968},
            {"playerAngle", 0x2958},
            {"playerAngleAux", 0x2954},
            {"mouseDownLeft", 0x42858},
            {"mouseDownRight", 0x4285A},
            {"mouseDown", 0x42C45},
            {"mapAngle", 0x1AC}
        };

        public static List<Wall> getWalls() {
            List<Wall> wallList = new List<Wall>();
            int offset = offsets["firstWall"];
            int wallCount = getWallCount();
            for (int i = 0; i < wallCount; i++) {
                IntPtr address = (IntPtr)vam.ReadInt32(BASE_POINTER) + offset + (i * 0x14);
                Wall w = new Wall();
                w.slot = vam.ReadInt32(address + 0x10);
                w.distance = vam.ReadInt32(address + 0x14);
                w.height = vam.ReadInt32(address + 0x18);
                if (w.height > 0 && w.distance < 1000000 && w.slot > -1)
                    wallList.Add(w);
            }
            return wallList;
        }

        public static void watchForHit() {
            while (true) {
                wasHit = vam.ReadInt32((IntPtr)
                vam.ReadInt32(
                    BASE_POINTER) + offsets["wasHit"]) == 1;
            }
        }

        public static void setSpin(bool spin) {
            new Task(() => {
                while (!spin) {
                    vam.WriteInt32((IntPtr)vam.ReadInt32(
                        BASE_POINTER) + offsets["mapAngle"], 0);
                }
            }).Start();
        }

        public static void StartAvoid(bool verbose) {
            while (true) {
                List<Wall> newWallList = getWalls();
                int slotCount = getSlotCount();
                Dictionary<int, int> possibleSlots = new Dictionary<int, int>();
                Console.Clear();
                foreach (Wall w in newWallList) {
                    Console.WriteLine(w);
                    int slot = w.slot;
                    int distance = w.distance;
                    int height = w.height;
                    if (slot < slotCount) {
                        if (possibleSlots.ContainsKey(slot)) {
                            if (distance < possibleSlots[slot]) {
                                //possibleSlots[slot] = distance;
                                possibleSlots[slot] = distance;
                            }
                        } else {
                            //possibleSlots[slot] = distance;
                            possibleSlots.Add(slot, distance);
                        }
                    }
                }
                for (int i = 0; i < 6; i++) {
                    if(i < slotCount && !possibleSlots.ContainsKey(i)) {
                        possibleSlots.Add(i, int.MaxValue);
                    }
                }
                Console.WriteLine("Player slot: " + getPlayerSlot());
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

        public static void makeDeathReport() {
            List<Wall> walls = getWalls();
            int playerSlot = getPlayerSlot();
            Wall hit = new Wall();
            hit.distance = int.MaxValue;
            foreach (Wall w in walls) {
                if (w.slot == playerSlot && w.distance - w.height < hit.distance - hit.height) {
                    hit = w;
                }
            }
            Console.Clear();
            Console.WriteLine("Hit " + hit);
            Console.ReadKey();
        }

        public static bool isAlive() {
            return vam.ReadInt32((IntPtr)
                    vam.ReadInt32(
                        BASE_POINTER) + offsets["deadBool"]) != 1;
        }

        public static bool setPlayerSlot(int slot) {
            int slotCount = getSlotCount();
            int angle = 360 / slotCount * (slot % slotCount) + (180 / slotCount);
            return vam.WriteInt32((IntPtr)vam.ReadInt32(
                        BASE_POINTER) + offsets["playerAngle"], angle) &&
            vam.WriteInt32((IntPtr)vam.ReadInt32(
                        BASE_POINTER) + offsets["playerAngleAux"], angle);
        }

        public static int getSlotCount() {
            return vam.ReadInt32((IntPtr)
                    vam.ReadInt32(
                        BASE_POINTER) + offsets["slotCount"]);
        }

        public static int getPlayerSlot() {
            int angle = getPlayerAngle();
            int slotCount = getSlotCount();
            return (int)Math.Round((double)angle / 360 * slotCount, 1);
        }

        public static int getPlayerAngle() {
            return vam.ReadInt32((IntPtr)
                    vam.ReadInt32(
                        BASE_POINTER) + offsets["playerAngle"]);
        }

        public static int getWallCount() {
            return vam.ReadInt32((IntPtr)
                    vam.ReadInt32(
                        BASE_POINTER) + offsets["wallCount"]);
        }

    }
}
