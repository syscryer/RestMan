using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace RestMan.CreateDbEntity
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "Data Source=|DataDirectory|D:\\Project\\JsonViewer\\JsonView\\bin\\debug\\DB\\RestMan.db;Pooling=true;FailIfMissing=false",//@"D:\Project\JsonViewer\RestMan.DbUtil\DB\RestMan.db",//必填, 数据库连接字符串
                DbType = SqlSugar.DbType.Sqlite,         //必填, 数据库类型
                IsAutoCloseConnection = true,       //默认false, 时候知道关闭数据库连接, 设置为true无需使用using或者Close操作
                InitKeyType = InitKeyType.SystemTable    //默认SystemTable, 字段信息读取, 如：该属性是不是主键，是不是标识列等等信息
            }))
            {
                db.DbFirst.CreateClassFile(@"D:\Project\JsonViewer\RestMan.Entity\Db", "RestMan.Entity.Db");

            };
            Console.WriteLine("生成完毕");
            Console.ReadKey();
        }
    }
}
