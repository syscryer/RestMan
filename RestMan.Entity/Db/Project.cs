using System;
using System.Linq;
using System.Text;

namespace RestMan.Entity.Db
{
    ///<summary>
    ///
    ///</summary>
    public partial class Project
    {
           public Project(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public int Id {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string GroupName {get;set;}

    }
}
