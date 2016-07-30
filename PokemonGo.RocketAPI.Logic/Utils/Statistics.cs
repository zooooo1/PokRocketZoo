﻿#region

using System;
using System.Linq;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.GeneratedCode;

#endregion


namespace PokemonGo.RocketAPI.Logic.Utils
{
    internal class Statistics
    {
        public static int TotalExperience;
        public static int TotalPokemons;
        public static int TotalItemsRemoved;
        public static int TotalPokemonsTransfered;
        public static int TotalStardust;
        public static string CurrentLevelInfos;
        public static int Currentlevel = -1;
        public static string PlayerName;

        public static DateTime InitSessionDateTime = DateTime.Now;
        public static TimeSpan Duration = DateTime.Now - InitSessionDateTime;

        public static async Task<string> _getcurrentLevelInfos(Inventory inventory)
        {
            var stats = await inventory.GetPlayerStats();
            var output = string.Empty;
            var stat = stats.FirstOrDefault();
            if (stat != null)
            {
                var ep = (stat.NextLevelXp - stat.PrevLevelXp) - (stat.Experience - stat.PrevLevelXp);
                var hours = Math.Round(ep/(TotalExperience / _getSessionRuntime()), 2);

                output = $"{stat.Level} (LvLUp in {hours}hours | {stat.Experience - stat.PrevLevelXp - GetXpDiff(stat.Level)}/{stat.NextLevelXp - stat.PrevLevelXp - GetXpDiff(stat.Level)} XP)";
                //output = $"{stat.Level} (LvLUp in {_hours}hours // EXP required: {_ep})";
            }
            return output;
        }

        public void SetUsername(GetPlayerResponse profile)
        {
            //PlayerName = profile.Profile.Username ?? "";
        }

        public static double _getSessionRuntime()
        {
            return (DateTime.Now - InitSessionDateTime).TotalSeconds/3600;
        }

        public static string _getSessionRuntimeInTimeFormat()
        {
            return (DateTime.Now - InitSessionDateTime).ToString(@"dd\.hh\:mm\:ss");
        }

        public void AddExperience(int xp)
        {
            TotalExperience += xp;
        }

        public void AddItemsRemoved(int count)
        {
            TotalItemsRemoved += count;
        }

        public void GetStardust(int stardust)
        {
            TotalStardust = stardust;
        }

        public void IncreasePokemons()
        {
            TotalPokemons += 1;
        }

        public void IncreasePokemonsTransfered()
        {
            TotalPokemonsTransfered += 1;
        }

        public async void UpdateConsoleTitle(Inventory _inventory)
        {
            CurrentLevelInfos = await _getcurrentLevelInfos(_inventory);
            Console.Title = ToString();
        }

        public override string ToString()
        {
            return
                string.Format(
                    "{0} - {1} - Lvl: {2:0} | EXP/H: {3:0} | P/H: {4:0} | Stardust: {5:0} | Transfered: {6:0} | Items Recycled: {7:0}",
                    PlayerName, _getSessionRuntimeInTimeFormat(), CurrentLevelInfos, TotalExperience / _getSessionRuntime(),
                    TotalPokemons / _getSessionRuntime(), TotalStardust, TotalPokemonsTransfered, TotalItemsRemoved);
        }

        public static int GetXpDiff(int level)
        {
            switch (level)
            {
                case 1:
                    return 0;
                case 2:
                    return 1000;
                case 3:
                    return 2000;
                case 4:
                    return 3000;
                case 5:
                    return 4000;
                case 6:
                    return 5000;
                case 7:
                    return 6000;
                case 8:
                    return 7000;
                case 9:
                    return 8000;
                case 10:
                    return 9000;
                case 11:
                    return 10000;
                case 12:
                    return 10000;
                case 13:
                    return 10000;
                case 14:
                    return 10000;
                case 15:
                    return 15000;
                case 16:
                    return 20000;
                case 17:
                    return 20000;
                case 18:
                    return 20000;
                case 19:
                    return 25000;
                case 20:
                    return 25000;
                case 21:
                    return 50000;
                case 22:
                    return 75000;
                case 23:
                    return 100000;
                case 24:
                    return 125000;
                case 25:
                    return 150000;
                case 26:
                    return 190000;
                case 27:
                    return 200000;
                case 28:
                    return 250000;
                case 29:
                    return 300000;
                case 30:
                    return 350000;
                case 31:
                    return 500000;
                case 32:
                    return 500000;
                case 33:
                    return 750000;
                case 34:
                    return 1000000;
                case 35:
                    return 1250000;
                case 36:
                    return 1500000;
                case 37:
                    return 2000000;
                case 38:
                    return 2500000;
                case 39:
                    return 1000000;
                case 40:
                    return 1000000;
            }
            return 0;
        }
    }
}