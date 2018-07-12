using System;
using System.Net.Mime;

namespace RestMan.DbUtil
{
    public class Config
    {
        public static string ConnectionString = "Data Source=|DataDirectory|"+AppContext.BaseDirectory+"DB\\RestMan.db;Pooling=true;FailIfMissing=false";
    }
}
