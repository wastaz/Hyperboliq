namespace Hyperboliq.FluentApi
{
    public static class Delete
    {
        public static DeleteFrom<TTable> From<TTable>()
        {
            return new DeleteFrom<TTable>(new DeleteExpression());
        }
    }
}
