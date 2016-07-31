using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using PokemonGo.RocketAPI.Enums;

namespace PokemonGo.RocketAPI.Conditions
{
    public class AuthCondition
    {
        public string AuthTypeStr { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public AuthType UserAuthType
        {
            get
            {
                return (AuthType)Enum.Parse(typeof(AuthType), AuthTypeStr, true);
            }
        }
    }

    public sealed class AuthConditionMap : CsvClassMap<AuthCondition>
    {
        public AuthConditionMap()
        {
            Map(m => m.AuthTypeStr).Index(0);
            Map(m => m.Username).Index(1);
            Map(m => m.Password).Index(2);
        }
    }
}
