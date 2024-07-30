using KHMI;
using KHMI.Types;

namespace TestDLLMod
{
    public class AdrenalineMod : KHMod
    {

        public AdrenalineMod(ModInterface mi) : base(mi)
        {
        }

        private int truncate(int current, int maxVal)
        {
            return Math.Clamp(current, 1, maxVal);
        }

        public override void playerLoaded(Entity newPlayer)
        {
            if(newPlayer.StatPage.MaxHP == 0)
            {
                newPlayer.StatPage.MaxHP = 1;
                newPlayer.StatPage.CurrentHP = 1;
            }
        }

        public override void onDamage(Entity target, int damage)
        {
            EntityTable et = EntityTable.Current(modInterface.dataInterface);
            Entity[] enemies = et.Enemies;
            
            if (target.IsPartyMember)
            {
                int newMaxHP = truncate(target.StatPage.MaxHP - (damage / 2), 255);
                if (newMaxHP == 1)
                {
                    newMaxHP = 0;
                }
                target.StatPage.MaxHP = newMaxHP;
            }

            else if (target.IsEnemy)
            {
                foreach (Entity e in enemies)
                {
                    if (target.StatPageID != e.StatPageID)
                    {
                        e.StatPage.MaxHP = truncate(e.StatPage.MaxHP + (damage / 2), 1500);
                        e.StatPage.CurrentHP = truncate(e.StatPage.CurrentHP + (damage / 2), 1500);
                        e.StatPage.Strength = truncate(e.StatPage.Strength + 1, 128);
                        e.StatPage.Defense = truncate(e.StatPage.Defense + 1, 80);
                    }
                }
            }
        }

        public override void onEntityDeath(Entity deceased)
        {
            if (deceased.IsPartyMember && !deceased.IsPlayer)
            {
                Entity player = Entity.getPlayer(modInterface.dataInterface);
                if (player != null)
                {
                    player.StatPage.MaxHP = truncate((player.StatPage.MaxHP * 9 / 10), 255);
                    player.StatPage.CurrentHP = truncate((player.StatPage.CurrentHP * 9 / 10), 255);

                }
            }
            else if (deceased.IsEnemy)
            {
                EntityTable et = EntityTable.Current(modInterface.dataInterface);
                Entity[] party = et.Party;
                foreach (Entity e in party)
                {
                    e.StatPage.MaxHP = truncate(e.StatPage.MaxHP + (deceased.StatPage.MaxHP / 150), 255);
                }
            }
        }
    }
}