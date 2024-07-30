using KHMI;
using KHMI.Types;

namespace TestDLLMod
{
    public class AdrenalineMod : KHMod
    {
        private int additionalPower = 0;
        public AdrenalineMod(ModInterface mi) : base(mi)
        {
        }

        private void updatePlayerPower(int power)
        {
            Entity player = Entity.getPlayer(modInterface.dataInterface);
            int difference = power - additionalPower;
            player.StatPage.Strength = truncate(player.StatPage.Strength + difference, 100);
            player.StatPage.Defense = truncate(player.StatPage.Defense + difference, 100);
            additionalPower = power;
        }

        private int truncate(int current, int maxVal)
        {
            return Math.Clamp(current, 1, maxVal);
        }

        public override void playerLoaded(Entity newPlayer)
        {
            newPlayer.StatPage.MaxHP = truncate(newPlayer.StatPage.MaxHP, int.MaxValue);
            newPlayer.StatPage.CurrentHP = truncate(newPlayer.StatPage.CurrentHP, int.MaxValue);
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
                if (target.IsPlayer)
                {
                    updatePlayerPower(0);
                }
            }

            else if (target.IsEnemy)
            {
                foreach (Entity e in enemies)
                {
                    if (target.StatPageID != e.StatPageID)
                    {
                        e.StatPage.MaxHP = truncate(e.StatPage.MaxHP + (damage / 5), 1500);
                        e.StatPage.CurrentHP = truncate(e.StatPage.CurrentHP + (damage / 5), 1500);
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
                    e.StatPage.MaxHP = truncate(e.StatPage.MaxHP + (deceased.StatPage.MaxHP / 150) + 1, 255);
                }
                updatePlayerPower(additionalPower + 1);
            }
        }
    }
}