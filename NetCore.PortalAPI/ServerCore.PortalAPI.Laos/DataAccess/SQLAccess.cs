using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netcore.Gateway.Interfaces;
using NetCore.Utils.Interfaces;
using ServerCore.PortalAPI;
using ServerCore.Utilities.Database;
using ServerCore.Utilities.Utils;

using System;

namespace ServerCore.DataAccess
{
    public class SQLAccess
    {
        private readonly AppSettings _settings;
        public static DBHelper _dbHeplerAuthen, _dbHeplerBilling, _dbHeplerAgency, _dbHeplerGuild, _dbHeplerGifcode;


        public SQLAccess(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public static DBHelper getAuthen()
        {
            if (_dbHeplerAuthen == null)
            {
                _dbHeplerAuthen = new DBHelper();
                _dbHeplerAuthen.SetConnectionString(Startup.settings.BillingAuthenticationAPIConnectionString);
            }

            return _dbHeplerAuthen;
        }
        public static DBHelper getBilling()
        {
            if (_dbHeplerBilling == null)
            {
                _dbHeplerBilling = new DBHelper();
                _dbHeplerBilling.SetConnectionString(Startup.settings.BillingDatabaseAPIConnectionString);
            }
            return _dbHeplerBilling;
        }
        public static DBHelper getAgency()
        {
            if (_dbHeplerAgency == null)
            {
                _dbHeplerAgency = new DBHelper();
                _dbHeplerAgency.SetConnectionString(Startup.settings.BillingAgencyAPIConnectionString);
            }
            return _dbHeplerAgency;
        }
        public static DBHelper getGuild()
        {
            if (_dbHeplerGuild == null)
            {
                _dbHeplerGuild = new DBHelper();
                _dbHeplerGuild.SetConnectionString(Startup.settings.BillingGuildAPIConnectionString);
            }
            return _dbHeplerGuild;
        }
        public static DBHelper getGifcode()
        {
            if (_dbHeplerGifcode == null)
            {
                _dbHeplerGifcode = new DBHelper();
                _dbHeplerGifcode.SetConnectionString(Startup.settings.BillingGifcodeAPIConnectionString);
            }
            return _dbHeplerGifcode;
        }
    }
}