namespace janzi.Logging;

public class JsonLogEntryPooledObjectPolicy : Microsoft.Extensions.ObjectPool.IPooledObjectPolicy<JsonLogEntry>
{
    public JsonLogEntry Create()
    {
        return new JsonLogEntry();
    }

    public bool Return(JsonLogEntry obj)
    {
        //make sure nothing scoped
        obj.Scope.Clear();
        return true;
    }
}