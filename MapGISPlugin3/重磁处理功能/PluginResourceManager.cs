// PluginResourceManager.cs
using MapGIS.GeoDataBase;
using System.Collections.Generic;

namespace MapGISPlugin3
{
    public static class PluginResourceManager
    {
        private static readonly List<DataBase> _openDatabases = new List<DataBase>();

        public static void RegisterDatabase(DataBase db)
        {
            if (db != null && !_openDatabases.Contains(db))
            {
                _openDatabases.Add(db);
            }
        }
    }
}