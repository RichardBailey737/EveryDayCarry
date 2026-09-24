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
     addhandler ensur.core.utilities.settings.Cache.GetSessionID, addressof GetSessionID
 End Sub

function GetSessionID()  as string
    return Httpcontext.current.Session.SessionID
End function

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

In this example I set the connection string according to a session variable set during login.  The SessionID is used to identify which user is executing the queries.  On a website, this is mandatory otherwise you will get conflicts with other users.   In a desktop app it's all running under the same user anyway.

The final step is optional.  The caching mechanism stores query results.  Results are stored for 3 minutes or until Session.instance.EndSession() is called.  It's recommended on a website to call on page unload.

Once configured, the connection string doesn't have to be specified again.  All database queries are started through the SessionCache singleton:

```C#
 var t = SessionCache.Instance.BySQL<bool>("select top 1 cast(allow_orig as bit) from dcs_doc_type_overlay where type_id = @0", "CACHENAME", 1);
 var obj = SessionCache.Instance.BySQL<CustomPoco>("select * from DCS_DOC  where DOC_ID = @0", "DOCCACHENAME", 123);
 var objList = SessionCache.Instance.BySQLList<CustomPoco>("select * from DCS_DOC  where CREATED_DATE > @0", "DOCS2026", '1/1/2026');
```

The first returns a strongly typed boolean based of the specified parameterized query.  If a query with the same cache name "CACHENAME" has been executed since this session, it returns the results in memory.  The second returns a custom object using PetaPoco to populate the object based on the property names (or PetaPoco Column attribute).  The third returns a list of objects (List<CustomPoco>).

Cached objects are stored by Object and cache name.  So a boolean with the cache named "CACHE1" and a string with the same name will not overlap.

The library offers a set of cache manipulation functions to manually load multiple, clear, set, and retrieve values.  It allows querying for single values, single objects, single value lists, object lists or execution of stored procedures.  All functions provide a way to pass a PetaPoco Sql object or a query string and list of parameters.

The stored procedure execution is a little different.  .NET requires you to know the name of the SQL parameters it's executing.  To simplify the process (which does require a little bit of overhead), it first queries the stored procedure using "sys.sp_sproc_columns" (so the user will need access to execute this stored procedure) to return a list of parameters and then matches those up sequentially with the parameter array.  If you would prefer not do use this, there is also an option to pass a Dictionary<string, object> with the parameter names instead.





