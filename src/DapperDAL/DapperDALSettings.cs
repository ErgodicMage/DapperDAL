﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace DapperDAL;

/// <summary>
/// Manages the Connection Strings outside of configuration so that DAOs only need the connection string name.
/// Though DI for the IConfiguration can be used to instantiate the DAOs that is not always needed or wanted (old code for example).
/// When DI is not needed then this class can be used instead.
/// This class allows for both IConfiguration and older ConfigurationManager. Depending on usage one of the Load functions
/// can be commented out. (Yeah I know not great practice, but it's practical for me)
/// </summary>
public static class DapperDALSettings
{
    public static string ConnectionStrings(string connectionName) =>
        DapperDALCache.Cache.Get(ConnectionKey(connectionName)) as string;


    public static string ConnectionKey(string key) => $"ConnectionString.{key}";

    /// <summary>
    /// Load connection string from a IConfiguration.
    /// </summary>
    /// <param name="config"></param>
    public static void Initialize(IConfiguration config)
    {
        DapperDALCache.Cache ??= new MemoryCache(new MemoryCacheOptions());

        var section = config.GetSection("ConnectionStrings").GetChildren();
        foreach (IConfigurationSection c in section)
        {
            //DapperDALCache.GetorSet(ConnectionKey(c.Key), c.Value);
            DapperDALCache.Cache.Set(ConnectionKey(c.Key), c.Value, 
                new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove});
        }
    }

    public static void Initialize(IDictionary<string, string> connectionStrings)
    {
        DapperDALCache.Cache ??= new MemoryCache(new MemoryCacheOptions());

        foreach (var c in connectionStrings)
        {
            //DapperDALCache.GetorSet(ConnectionKey(c.Key), c.Value);
            DapperDALCache.Cache.Set(ConnectionKey(c.Key), c.Value, 
                new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove});
        }
    }

    /// <summary>
    /// Load connection strings from older ConfigurationManager.ConnectionStrings
    /// </summary>
    //public static void Load()
    //{
    //    var settings = System.Configuration.ConfigurationManager.ConnectionStrings;

    //    foreach (System.Configuration.ConnectionStringSettings setting in settings)
    //    {
    //        if (!ConnectionStrings.ContainsKey(setting.Name))
    //            ConnectionStrings.Add(setting.Name, setting.ConnectionString);
    //    }
    //}

    /// <summary>
    /// Setting to have strings from columns of type char[] trimmed to remove the trailing spaces.
    /// </summary>
    public static void TrimStrings(bool on = true)
    {
        if (on && !SqlMapper.HasTypeHandler(typeof(TrimmedStringHandler)))
            SqlMapper.AddTypeHandler(new TrimmedStringHandler());
        else if (!on && SqlMapper.HasTypeHandler(typeof(TrimmedStringHandler)))
            SqlMapper.RemoveTypeMap(typeof(TrimmedStringHandler));
    }

    public static void BooleanYNConverter(bool on = true)
    {
        if (on && !SqlMapper.HasTypeHandler(typeof(BooleanYNHandler)))
            SqlMapper.AddTypeHandler(new BooleanYNHandler());
        else if (!on && SqlMapper.HasTypeHandler(typeof(BooleanYNHandler)))
            SqlMapper.RemoveTypeMap(typeof(BooleanYNHandler));
    }
}
