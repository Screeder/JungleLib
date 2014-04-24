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
        private static JungleMobs[] jungleMobs = new JungleMobs[12];
        private static List<Obj_AI_Minion> jungleMobList = new List<Obj_AI_Minion>();

        public class JungleMobs
        {
            public JungleMobs(String name, bool smite, bool buff)
            {
                this.name = name;
                this.smite = smite;
                this.buff = buff;
            }

            public String name;
            public bool smite;
            public bool buff;
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

        public static bool HasBuff(Obj_AI_Minion jungleBigMob)
        {
            foreach (JungleMobs jungleMob in jungleMobs)
            {
                if (jungleBigMob.Name.Contains(jungleMob.name))
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

        static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsValid && sender.Type == GameObjectType.obj_AI_Minion
                && sender.Team == GameObjectTeam.Neutral)
            {
                jungleMobList.Add((Obj_AI_Minion)sender);
            }
        }

        public static void InitJungleMobs()
        {
            if (initJungleMobs)
                return;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            foreach (Obj_AI_Minion objAiMinion in ObjectManager.Get<Obj_AI_Minion>())
            {
                Obj_AI_Base_OnCreate(objAiMinion, new EventArgs());
            }
            jungleMobs[0] =  new JungleMobs("GreatWraith",  true,   false);
            jungleMobs[1] =  new JungleMobs("AncientGolem", true,   true);
            jungleMobs[2] =  new JungleMobs("GiantWolf",    true,   false);
            jungleMobs[3] =  new JungleMobs("Wraith",       true,   false);
            jungleMobs[4] =  new JungleMobs("LizardElder",  true,   true);
            jungleMobs[5] =  new JungleMobs("Golem",        true,   false);
            jungleMobs[6] =  new JungleMobs("Worm",         true,   true);
            jungleMobs[7] =  new JungleMobs("Dragon",       true,   false);
            jungleMobs[8] =  new JungleMobs("YoungLizard",  false,  false);
            jungleMobs[9] =  new JungleMobs("Wolf",         false,  false);
            jungleMobs[10] = new JungleMobs("LesserWraith", false,  false);
            jungleMobs[11] = new JungleMobs("Golem",        false,  false);
            initJungleMobs = true;
        }
    }
}