#region

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Logging;
using PokemonGo.RocketAPI.Conditions;
using CsvHelper;
using CsvHelper.Configuration;

#endregion


namespace PokemonGo.RocketAPI.Console
{
    public class Settings : ISettings
    {
        private AuthCondition _userAuthCondition;
        private ICollection<PokemonId> _pokemonsToEvolve;
        private ICollection<PokemonId> _pokemonsNotToTransfer;
        private ICollection<PokemonId> _pokemonsNotToCatch;
        private IDictionary<PokemonId, PokemonKeepCondition> _pokemonsToKeepCondition;
        private IEnumerable<LocationCondition> _locationsCondition;
        private ICollection<PokemonId> _snipePokemons;
        private IEnumerable<Location> _snipeLocations;

        private double _latitude = 0;
        private double _longtitude = 0;
        private double _altitude = 0;
        private int _radius = 500;
        private double _walkSpeed = 30;

        public string GoogleRefreshToken
        {
            get { return UserSettings.Default.GoogleRefreshToken; }
            set
            {
                UserSettings.Default.GoogleRefreshToken = value;
                UserSettings.Default.Save();
            }
        }

        public void setDefaultLocation(LocationCondition location)
        {
            _latitude = location.Latitude;
            _longtitude = location.Longitude;
            _altitude = location.Altitude;
            _walkSpeed = location.WalkingSpeedInKmPerHour;
            _radius = (int)location.Radius;
        }

        // original (might be dummy)
        public AuthType AuthType { get { return UserAuthCondition.UserAuthType; } }
        public string PtcPassword { get { return UserAuthCondition.Password; } }
        public string PtcUsername { get { return UserAuthCondition.Username; } }
        public string GoogleEmail { get { return UserAuthCondition.Username;  } }
        public string GooglePassword { get { return UserAuthCondition.Password; } }
        public double DefaultLatitude { get { return _latitude == 0 ? LocationsCondition.First().Latitude : _latitude; } set { _latitude = value; } }
        public double DefaultLongitude { get { return _longtitude == 0 ? LocationsCondition.First().Longitude : _longtitude; } set { _longtitude = value; } }
        public double DefaultAltitude { get { return _altitude == 0 ? LocationsCondition.First().Altitude : _altitude; } set { _altitude = value; } }
        public bool UseGPXPathing { get { return false; } }
        public string GPXFile { get { return "";  } }
        public bool GPXIgnorePokestops { get { return false; } }
        public double WalkingSpeedInKilometerPerHour { get { return _walkSpeed; } set { _walkSpeed = value; } }
        public int MaxTravelDistanceInMeters { get { return _radius; } set { _radius = value; } }

        public bool UsePokemonToNotCatchList { get { return true; } }
        public bool UsePokemonToNotTransferList { get { return true; } }
        public bool UsePokemonToEvolveList { get { return true; } }
        public bool CatchPokemon { get { return true; } }

        public bool EvolvePokemon { get { return false; } }
        public bool EvolveOnlyPokemonAboveIV { get { return true; } }
        public float EvolveOnlyPokemonAboveIVValue { get { return 95; } }
        public int EvolveKeepCandiesValue { get { return 100; } }

        public bool TransferPokemon { get { return true; } }
        public int TransferPokemonKeepDuplicateAmount { get { return 1; } }
        public bool NotTransferPokemonsThatCanEvolve { get { return false; } }
        public bool UseTransferPokemonKeepAboveCP { get { return true; } }
        public int TransferPokemonKeepAboveCP { get { return 1000; } }
        public bool UseTransferPokemonKeepAboveIV { get { return true; } }
        public float TransferPokemonKeepAboveIVPercentage { get { return 85; } }

        public bool PrioritizeIVOverCP { get { return false; } }
        public bool UseLuckyEggs { get { return false; } }
        public bool UseIncense { get { return false; } }
        public bool DebugMode { get { return true; } }
        public bool UseTeleportInsteadOfWalking { get { return false; } }

        // read settings
        public ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter => new[]
        {
            new KeyValuePair<ItemId, int>(ItemId.ItemUnknown, 0),
            new KeyValuePair<ItemId, int>(ItemId.ItemPokeBall, 20),
            new KeyValuePair<ItemId, int>(ItemId.ItemGreatBall, 20),
            new KeyValuePair<ItemId, int>(ItemId.ItemUltraBall, 50),
            new KeyValuePair<ItemId, int>(ItemId.ItemMasterBall, 100),

            new KeyValuePair<ItemId, int>(ItemId.ItemPotion, 0),
            new KeyValuePair<ItemId, int>(ItemId.ItemSuperPotion, 0),
            new KeyValuePair<ItemId, int>(ItemId.ItemHyperPotion, 20),
            new KeyValuePair<ItemId, int>(ItemId.ItemMaxPotion, 50),

            new KeyValuePair<ItemId, int>(ItemId.ItemRevive, 10),
            new KeyValuePair<ItemId, int>(ItemId.ItemMaxRevive, 50),

            new KeyValuePair<ItemId, int>(ItemId.ItemLuckyEgg, 200),

            new KeyValuePair<ItemId, int>(ItemId.ItemIncenseOrdinary, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncenseSpicy, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncenseCool, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncenseFloral, 100),

            new KeyValuePair<ItemId, int>(ItemId.ItemTroyDisk, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemXAttack, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemXDefense, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemXMiracle, 100),

            new KeyValuePair<ItemId, int>(ItemId.ItemRazzBerry, 20),
            new KeyValuePair<ItemId, int>(ItemId.ItemBlukBerry, 10),
            new KeyValuePair<ItemId, int>(ItemId.ItemNanabBerry, 10),
            new KeyValuePair<ItemId, int>(ItemId.ItemWeparBerry, 30),
            new KeyValuePair<ItemId, int>(ItemId.ItemPinapBerry, 30),

            new KeyValuePair<ItemId, int>(ItemId.ItemSpecialCamera, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasicUnlimited, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasic, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemPokemonStorageUpgrade, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemItemStorageUpgrade, 100),
        };

        public ICollection<PokemonId> PokemonsToEvolve
        {
            get
            {
                //Type of pokemons to evolve
                _pokemonsToEvolve = _pokemonsToEvolve ?? LoadPokemonList("PokemonsToEvolve");
                return _pokemonsToEvolve;
            }
        }

        public ICollection<PokemonId> PokemonsToNotTransfer
        {
            get
            {
                //Type of pokemons not to transfer
                _pokemonsNotToTransfer = _pokemonsNotToTransfer ?? LoadPokemonList("PokemonsNotToTransfer");
                return _pokemonsNotToTransfer;
            }
        }

        public ICollection<PokemonId> PokemonsToNotCatch
        {
            get
            {
                //Type of pokemons not to catch
                _pokemonsNotToCatch = _pokemonsNotToCatch ?? LoadPokemonList("PokemonsNotToCatch");
                return _pokemonsNotToCatch;
            }
        }

        private ICollection<PokemonId> LoadPokemonList(string filename)
        {
            ICollection<PokemonId> result = new List<PokemonId>();
            string path = Directory.GetCurrentDirectory() + "\\" + UserSettings.Default.ConfigPath + "\\";
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            if (!File.Exists(path + filename + ".txt"))
            {
                string pokemonName = Properties.Resources.ResourceManager.GetString(filename);
                Logger.Write($"File: {filename} not found, creating new...", LogLevel.Warning);
                File.WriteAllText(path + filename + ".txt", pokemonName);
            }
            if (File.Exists(path + filename + ".txt"))
            {
                Logger.Write($"Loading File: {UserSettings.Default.ConfigPath}\\{filename}", LogLevel.Info);
                string[] _locallist = File.ReadAllLines(path + filename + ".txt");
                foreach (string pokemonName in _locallist)
                {
                    var pokemon = Enum.Parse(typeof(PokemonId), pokemonName, true);
                    if (pokemonName != null) result.Add((PokemonId)pokemon);
                }
            }
            return result;
        }

        public IDictionary<PokemonId, PokemonKeepCondition> PokemonsToKeepCondition
        {
            get
            {
                //Type of pokemons not to catch
                _pokemonsToKeepCondition = _pokemonsToKeepCondition ?? LoadPokemonKeepCsv("PokemonsToKeep");
                return _pokemonsToKeepCondition;
            }
        }

        private IDictionary<PokemonId, PokemonKeepCondition> LoadPokemonKeepCsv(string filename)
        {
            IDictionary<PokemonId, PokemonKeepCondition> result = new Dictionary<PokemonId, PokemonKeepCondition>();
            string path = Directory.GetCurrentDirectory() + "\\" + UserSettings.Default.ConfigPath + "\\";
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            if (File.Exists(path + filename + ".csv"))
            {
                Logger.Write($"Loading File: {UserSettings.Default.ConfigPath}\\{filename}", LogLevel.Info);
                using (var sr = new StreamReader(path + filename + ".csv"))
                using (var csv = new CsvHelper.CsvReader(sr))
                {
                    csv.Configuration.RegisterClassMap<PokemonKeepConditionMap>();
                    var records = csv.GetRecords<PokemonKeepCondition>();
                    foreach (var record in records)
                    {
                        result[(PokemonId)record.Id] = record;
                    }
                }
            }
            return result;
        }

        public IEnumerable<LocationCondition> LocationsCondition
        {
            get
            {
                // move locations
                _locationsCondition = _locationsCondition ?? LoadLocationConditionCsv("MoveLocations");
                return _locationsCondition;
            }
        }

        private IEnumerable<LocationCondition> LoadLocationConditionCsv(string filename)
        {
            List<LocationCondition> result = new List<LocationCondition>();
            string path = Directory.GetCurrentDirectory() + "\\" + UserSettings.Default.ConfigPath + "\\";
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            if (File.Exists(path + filename + ".csv"))
            {
                Logger.Write($"Loading File: {UserSettings.Default.ConfigPath}\\{filename}", LogLevel.Info);
                using (var sr = new StreamReader(path + filename + ".csv"))
                using (var csv = new CsvHelper.CsvReader(sr))
                {
                    csv.Configuration.RegisterClassMap<LocationConditionMap>();
                    var records = csv.GetRecords<LocationCondition>();
                    foreach (var record in records)
                    {
                        result.Add(record);
                    }
                }
            }
            return result;
        }

        public AuthCondition UserAuthCondition
        {
            get
            {
                //Type of pokemons not to catch
                _userAuthCondition = _userAuthCondition ?? LoadAuthCondition("Auth");
                return _userAuthCondition;
            }
        }

        private AuthCondition LoadAuthCondition(string filename)
        {
            AuthCondition result = new AuthCondition();
            string path = Directory.GetCurrentDirectory() + "\\" + UserSettings.Default.AuthPath;
            if (File.Exists(path))
            {
                Logger.Write($"Loading File: {UserSettings.Default.AuthPath}", LogLevel.Info);
                using (var sr = new StreamReader(path))
                using (var csv = new CsvHelper.CsvReader(sr))
                {
                    csv.Configuration.RegisterClassMap<AuthConditionMap>();
                    var records = csv.GetRecords<AuthCondition>();
                    foreach (var record in records)
                    {
                        result = record;
                    }
                }
            }
            return result;
        }

        public ICollection<PokemonId> SnipePokemons
        {
            get
            {
                //Type of pokemons to snipe
                _snipePokemons = _snipePokemons ?? LoadPokemonList("SnipePokemons");
                return _snipePokemons;
            }
        }

        public IEnumerable<Location> SnipeLocations
        {
            get
            {
                // snipe locations
                _snipeLocations = _snipeLocations ?? LoadLocationCsv("SnipeLocations");
                return _snipeLocations;
            }
        }

        private IEnumerable<Location> LoadLocationCsv(string filename)
        {
            List<Location> result = new List<Location>();
            string path = Directory.GetCurrentDirectory() + "\\" + UserSettings.Default.ConfigPath + "\\";
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            if (File.Exists(path + filename + ".csv"))
            {
                Logger.Write($"Loading File: {UserSettings.Default.ConfigPath}\\{filename}", LogLevel.Info);
                using (var sr = new StreamReader(path + filename + ".csv"))
                using (var csv = new CsvHelper.CsvReader(sr))
                {
                    csv.Configuration.RegisterClassMap<LocationMap>();
                    var records = csv.GetRecords<Location>();
                    foreach (var record in records)
                    {
                        result.Add(record);
                    }
                }
            }
            return result;
        }

    }
}
