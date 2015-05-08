namespace Hyperboliq.FluentApi
{
    public static class Delete
    {
        public static DeleteFrom<TTable> From<TTable>() => new DeleteFrom<TTable>();
    }
}
