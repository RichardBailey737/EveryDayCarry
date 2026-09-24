# EveryDayCarry

## Project Overview
This is a collection of utilities created to help with a variety of tasks like database access, caching, AI and plugins.  

### Individual libraries

#### SessionCache - Ensur.Core.Utilities\Database\LocalDBCache.cs

This is my main database access library built to solve two problems.  

1) I grew tired of repeating the same code to open a connection to a database.  The process often doesn't change much so it felt redundant to have to repeatedly tell it which connection to use and how and when to open/close connections.  Not to mention the amount of code overhead to send a parametrized query or query a stored procedure was pretty high.  I also grew frustrated with the Entity framework.  It was easy to let your database definition become out of sync.  The objects created by the EntityFramework are difficult to use elsewhere with all the extra properties and events.  I enjoy the flexibility and simplicity of PetaPoco.
2) Caching.  I frequently saw multiple layers of code accessing the same data over and over again, each time re-querying the database.  Passing objects back and forth between calls can be tedious and looks clumsy.  I wanted a simpler more elegant solution.

The library has one major requirement:   You have to add a handler to the Ensur.Core.Utilities.Settings.ConnectionString.GetRepository event.  That handler should return the name of the connection string to use (in the app.config or web.config).  

In a desktop app this is pretty straight forward 

Somewhere in the initialization add the handler:
```C#
Ensur.Core.Utilities.Settings.ConnectionString.GetRepository += ConnectionString_GetRepository;
```

then return the name of the connection string in the handler:

```C#
private string ConnectionString_GetRepository()
{
    return "DEV2";
}
```

In a web application you can create an [IIS handler](https://go.microsoft.com/?linkid=8101007) to add the handler at the creation of a user session:

```VB.Net
 Public Sub SetRepositoryEventHandler(ByVal source As Object, ByVal e As EventArgs) Handles _context.AcquireRequestState
     AddHandler Ensur.Core.Utilities.Settings.ConnectionString.GetRepository, AddressOf SetRepository
 End Sub

Function SetRepository() As String
    If Not HttpContext.Current Is Nothing Then
        If HttpContext.Current.Session("repository") Is Nothing Then
            Return ConfigurationManager.ConnectionStrings(0).Name
        Else
            Return HttpContext.Current.Session("repository")
        End If
    Else
        Return ""
    End If
End Function
```

In this example I set the connection string according to a session variable set during login.  

Once configured, the connection string doesn't have to be specified again.  All database queries are started through the SessionCache singleton:

```C#
 var t = SessionCache.Instance.BySQL<bool>("select top 1 cast(allow_orig as bit) from dcs_doc_type_overlay where type_id = @0", "CACHENAME", 1);
 var obj = SessionCache.Instance.BySQL<CustomPoco>("select * from DCS_DOC  where DOC_ID = @0", "DOCCACHENAME", 123);
 var objList = SessionCache.Instance.BySQLList<CustomPoco>("select * from DCS_DOC  where CREATED_DATE > @0", "DOCS2026", '1/1/2026');
```

The first returns a strongly typed boolean based of the specified parameterized query.  If the 



