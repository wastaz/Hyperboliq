namespace Hyperboliq
{
    public class ExpressionParameter<TParamType> : ExpressionParameter
    {
        public ExpressionParameter(string name) : base(name) { }

        public void SetValue(TParamType value)
        {
            base.SetValue(value);
        }

        public static implicit operator TParamType(ExpressionParameter<TParamType> parameter)
        {
            return default(TParamType);
        }
    }
}
