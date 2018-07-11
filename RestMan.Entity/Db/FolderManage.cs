using System;
using System.Linq;
using System.Text;

namespace RestMan.Entity.Db
{
    ///<summary>
    ///
    ///</summary>
    public partial class FolderManage
    {
           public FolderManage(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public int ID {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? PID {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string FolderName {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? GroupId {get;set;}

    }
}
