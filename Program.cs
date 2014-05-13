using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using LeagueSharp;
using SharpDX;

namespace JungleLib
{

    public class Jungle
    {
        private static bool initJungleMobs = false;
        private static List<JungleMobs> jungleMobs = new List<JungleMobs>();
        private static List<JungleCamps> jungleCamps= new List<JungleCamps>();
        private static List<Obj_AI_Minion> jungleMobList = new List<Obj_AI_Minion>();

        public class JungleMobs
        {
            public JungleMobs(String name, bool smite, bool buff, bool boss, int mapID)
            {
                this.name = name;
                this.smite = smite;
                this.buff = buff;
                this.boss = boss;
                this.mapID = mapID;
            }

            public String name;
            public bool smite;
            public bool buff;
            public bool boss;
            public int mapID;
        }

        public class JungleCamps
        {
            public JungleCamps(String name, GameObjectTeam team, int campID, int spawnTime, int respawnTime, int mapID, Vector3 mapPosition, Vector3 minimapPosition, JungleMobs[] creeps)
            {
                this.name = name;
                this.team = team;
                this.campID = campID;
                this.spawnTime = spawnTime;
                this.respawnTime = respawnTime;
                this.mapID = mapID;
                this.mapPosition = mapPosition;
                this.minimapPosition = minimapPosition;
                this.creeps = creeps;
                nextRespawnTime = 0;
            }

            public String name;
            public GameObjectTeam team;
            public int campID;
            public int spawnTime;
            public int respawnTime;
            public int nextRespawnTime;
            public int mapID;
            public Vector3 mapPosition;
            public Vector3 minimapPosition;
            public JungleMobs[] creeps;
        }

        static Jungle()
        {
            InitJungleMobs();
        }

        protected static void DebugMode()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

        }

        private static bool ValidJungleMob(Obj_AI_Minion jungleMob)
        {
            return jungleMob != null && jungleMob.IsValid && !jungleMob.IsDead && jungleMob.Team == GameObjectTeam.Neutral;
        }

        public static bool IsBigMob(Obj_AI_Minion jungleBigMob)
        {
            foreach (JungleMobs jungleMob in jungleMobs)
            {
                if (jungleBigMob.Name.Contains(jungleMob.name))
                {
                    return jungleMob.smite;
                }
            }
            return false;
        }

        public static bool IsBossMob(Obj_AI_Minion jungleBossMob)
        {
            foreach (JungleMobs jungleMob in jungleMobs)
            {
                if (jungleBossMob.SkinName.Contains(jungleMob.name))
                {
                    return jungleMob.boss;
                }
            }
            return false;
        }

        public static bool HasBuff(Obj_AI_Minion jungleBigMob)
        {
            foreach (JungleMobs jungleMob in jungleMobs)
            {
                if (jungleBigMob.SkinName.Contains(jungleMob.name))
                {
                    return jungleMob.buff;
                }
            }
            return false;
        }

        public static List<Obj_AI_Minion> GetJungleMobs(bool visible, float range)
        {

            List<Obj_AI_Minion> jungleMobsDelete = new List<Obj_AI_Minion>();
            List<Obj_AI_Minion> jungleMobs = new List<Obj_AI_Minion>();
            
            foreach (Obj_AI_Minion jungleMob in jungleMobList)
            {
                if (ValidJungleMob(jungleMob))
                {
                    if (visible && jungleMob.IsVisible)
                    {
                        jungleMobs.Add(jungleMob);
                    }
                    else if (range.CompareTo(0) != 0 &&
                             Utils.GetDistanceSqr(jungleMob.Position, ObjectManager.Player.Position) <=
                             Math.Pow(range, 2))
                    {
                        jungleMobs.Add(jungleMob);
                    }
                    else
                    {
                        jungleMobs.Add(jungleMob);
                    }
                }
                else
                {
                    jungleMobsDelete.Add(jungleMob);
                }
            }
            foreach (Obj_AI_Minion jungleMob in jungleMobsDelete)
            {
                if (!ValidJungleMob(jungleMob))
                {
                    jungleMobList.Remove(jungleMob);
                }
            }
            return jungleMobs;
        }

        public static List<JungleCamps> GetAvailableJungleCamps()
        {
            List<JungleCamps> jungleCamps1= new List<JungleCamps>();
            foreach (JungleCamps jungleCamp in jungleCamps)
            {
                if (jungleCamp.nextRespawnTime == 0)
                {
                    jungleCamps1.Add(jungleCamp);
                }
            }
            return jungleCamps1;
        }

        public static List<JungleCamps> GetJungleCamps()
        {
            return jungleCamps;
        }

        static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsValid && sender.Type == GameObjectType.obj_AI_Minion
                && sender.Team == GameObjectTeam.Neutral)
            {
                jungleMobList.Add((Obj_AI_Minion)sender);
            }
        }

        private static JungleMobs GetJungleMobByName(string name)
        {
            return jungleMobs.Find(delegate(JungleMobs JM)
                {
                    return JM.name == name;
                });
        }

        private static JungleCamps GetJungleCampByID(int id)
        {
            return jungleCamps.Find(delegate(JungleCamps JM)
            {
                return JM.campID == id;
            });
        }

        private static void UpdateCamps(int networkID, int campID, byte emptyType)
        {
            if (emptyType != 3)
            {
                JungleCamps jungleCamp = GetJungleCampByID(campID);
                if (jungleCamp != null)
                {
                    jungleCamp.nextRespawnTime = (int)Game.Time + jungleCamp.respawnTime;
                }
            }
        }

        public static void InitJungleMobs()
        {
            if (initJungleMobs)
                return;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Game.OnGameUpdate += Game_OnGameUpdate;
            foreach (Obj_AI_Minion objAiMinion in ObjectManager.Get<Obj_AI_Minion>())
            {
                Obj_AI_Base_OnCreate(objAiMinion, new EventArgs());
            }
            jungleMobs.Add(new JungleMobs("GreatWraith", true, false, false, 0));
            jungleMobs.Add(new JungleMobs("AncientGolem", true, true, false, 0));
            jungleMobs.Add(new JungleMobs("GiantWolf", true, false, false, 0));
            jungleMobs.Add(new JungleMobs("Wraith", true, false, false, 0));
            jungleMobs.Add(new JungleMobs("LizardElder", true, true, false, 0));
            jungleMobs.Add(new JungleMobs("Golem", true, false, false, 0));
            jungleMobs.Add(new JungleMobs("Worm", true, true, true, 0));
            jungleMobs.Add(new JungleMobs("Dragon", true, false, true, 0));
            jungleMobs.Add(new JungleMobs("Wight", true, false, false, 0));
            jungleMobs.Add(new JungleMobs("YoungLizard", false, false, false, 0));
            jungleMobs.Add(new JungleMobs("Wolf", false, false, false, 0));
            jungleMobs.Add(new JungleMobs("LesserWraith", false, false, false, 0));
            jungleMobs.Add(new JungleMobs("SmallGolem", false, false, false, 0));
            jungleMobs.Add(new JungleMobs("TT_Spiderboss", true, true, true, 1)); //Wenn map eingefügt ist IDS anpassen und Cmap TT hinzufügen

            jungleCamps.Add(new JungleCamps("blue", GameObjectTeam.Order, 1, 115, 300, 0, new Vector3(3570, 7670, 54), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("AncientGolem"), GetJungleMobByName("YoungLizard"), GetJungleMobByName("YoungLizard") }));
            jungleCamps.Add(new JungleCamps("wolves", GameObjectTeam.Order, 2, 125, 50, 0, new Vector3(3430, 6300, 56), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("GiantWolf"), GetJungleMobByName("Wolf"), GetJungleMobByName("Wolf") }));
            jungleCamps.Add(new JungleCamps("wraiths", GameObjectTeam.Order, 3, 125, 50, 0, new Vector3(6540, 5230, 56), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("Wraith"), GetJungleMobByName("LesserWraith"), GetJungleMobByName("LesserWraith"), GetJungleMobByName("LesserWraith") }));
            jungleCamps.Add(new JungleCamps("red", GameObjectTeam.Order, 4, 115, 300, 0, new Vector3(7370, 3830, 58), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("LizardElder"), GetJungleMobByName("YoungLizard"), GetJungleMobByName("YoungLizard") }));
            jungleCamps.Add(new JungleCamps("golems", GameObjectTeam.Order, 5, 125, 50, 0, new Vector3(7990, 2550, 54), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("Golem"), GetJungleMobByName("SmallGolem") }));
            jungleCamps.Add(new JungleCamps("wight", GameObjectTeam.Order, 13, 125, 50, 0, new Vector3(12266, 6215, 54), new Vector3(), new JungleMobs[] { GetJungleMobByName("Wight") }));
            jungleCamps.Add(new JungleCamps("blue", GameObjectTeam.Chaos, 7, 115, 300, 0, new Vector3(10455, 6800, 55), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("AncientGolem"), GetJungleMobByName("YoungLizard"), GetJungleMobByName("YoungLizard") }));
            jungleCamps.Add(new JungleCamps("wolves", GameObjectTeam.Chaos, 8, 125, 50, 0, new Vector3(10570, 8150, 63), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("GiantWolf"), GetJungleMobByName("Wolf"), GetJungleMobByName("Wolf") }));
            jungleCamps.Add(new JungleCamps("wraiths", GameObjectTeam.Chaos, 9, 125, 50, 0, new Vector3(7465, 9220, 56), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("Wraith"), GetJungleMobByName("LesserWraith"), GetJungleMobByName("LesserWraith"), GetJungleMobByName("LesserWraith") }));
            jungleCamps.Add(new JungleCamps("red", GameObjectTeam.Chaos, 10, 115, 300, 0, new Vector3(6620, 10637, 55), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("LizardElder"), GetJungleMobByName("YoungLizard"), GetJungleMobByName("YoungLizard") }));
            jungleCamps.Add(new JungleCamps("golems", GameObjectTeam.Chaos, 11, 125, 50, 0, new Vector3(6010, 11920, 40), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("Golem"), GetJungleMobByName("SmallGolem") }));
            jungleCamps.Add(new JungleCamps("wight", GameObjectTeam.Chaos, 14, 125, 50, 0, new Vector3(1688, 8248, 54), new Vector3(), new JungleMobs[] { GetJungleMobByName("Wight") }));
            jungleCamps.Add(new JungleCamps("dragon", GameObjectTeam.Neutral, 6, 2 * 60 + 30, 360, 0, new Vector3(9400, 4130, -61), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("Dragon") }));
            jungleCamps.Add(new JungleCamps("nashor", GameObjectTeam.Neutral, 12, 15 * 60, 420, 0, new Vector3(4620, 10265, -63), new Vector3(3670, 7520, 54), new JungleMobs[] { GetJungleMobByName("Worm") }));

            foreach (JungleCamps jungleCamp in jungleCamps)
            {
                int nextRespawnTime = jungleCamp.spawnTime - (int)Game.Time;
                if (nextRespawnTime > 0)
                {
                    jungleCamp.nextRespawnTime = nextRespawnTime;
                }
            }

            initJungleMobs = true;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            foreach (JungleCamps jungleCamp in jungleCamps)
            {
                if ((jungleCamp.nextRespawnTime - (int)Game.Time) < 0)
                {
                    jungleCamp.nextRespawnTime = 0;
                }
            }
        }

        private static void EmptyCamp(BinaryReader b)
        {
            int nwID = 0;
            int cID = 0;
            byte emptyType = 0;
            byte[] h = b.ReadBytes(4);
            nwID = BitConverter.ToInt32(h, 0);

            h = b.ReadBytes(4);
            cID = BitConverter.ToInt32(h, 0);

            emptyType = b.ReadByte();
            UpdateCamps(nwID, cID, emptyType);
        }

        static void Game_OnGameProcessPacket(GamePacketProcessEventArgs args)
        {
                try
                {
                    MemoryStream stream = new MemoryStream(args.PacketData);
                    using (BinaryReader b = new BinaryReader(stream))
                    {
                        Boolean targetPKT = false;
                        int pos = 0;
                        int length = (int) b.BaseStream.Length;
                        while (pos < length)
                        {
                            int v = b.ReadInt32();
                            if (v == 194)
                            {
                                byte[] h = b.ReadBytes(1);
                                EmptyCamp(b);
                            }
                            pos += sizeof (int);
                        }
                    }
                }
                catch (EndOfStreamException e)
                {
                }
        }
    }
}