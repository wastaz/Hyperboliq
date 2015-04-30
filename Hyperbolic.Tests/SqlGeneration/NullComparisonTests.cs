using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGenerator;
using FluentAssertions;

namespace Hyperboliq.Tests.SqlGeneration
{
	[Trait("SqlGeneration", "NullComparisons")]
	public class NullComparisonTests
	{
		[Fact]
		public void ItCanGenerateCorrectSqlForAComparisonWithNull()
		{
			var stream =
				StreamFrom(
					Kw(KeywordNode.Select),
					Col<Person>("*"),
					Kw(KeywordNode.From),
					Tbl<Person>(),
					Kw(KeywordNode.Where),
					BinExp(Col<Person>("Age"), BinaryOperation.Equal, Null()));
			var result = SqlifySeq(AnsiSql.Dialect, stream);

			result.Should().Be("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NULL");
		}

		[Fact]
		public void ItCanGenerateCorrectSqlForAnInvertedComparisonWithNull()
		{
			var stream =
				StreamFrom(
					Kw(KeywordNode.Select),
					Col<Person>("*"),
					Kw(KeywordNode.From),
					Tbl<Person>(),
					Kw(KeywordNode.Where),
					BinExp(Col<Person>("Age"), BinaryOperation.NotEqual, Null()));
			var result = SqlifySeq(AnsiSql.Dialect, stream);

			result.Should().Be("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NOT NULL");
		}

		[Fact]
		public void ItCanSupportFlippingOrdersForComparisonWithNull()
		{
			var stream =
				StreamFrom(
					Kw(KeywordNode.Select),
					Col<Person>("*"),
					Kw(KeywordNode.From),
					Tbl<Person>(),
					Kw(KeywordNode.Where),
					BinExp(Null(), BinaryOperation.Equal, Col<Person>("Age")));
			var result = SqlifySeq(AnsiSql.Dialect, stream);

			result.Should().Be("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NULL");
		}

		[Fact]
		public void ItCanSupportFlippingOrdersForInvertedComparisonWithNull()
		{
			var stream =
				StreamFrom(
					Kw(KeywordNode.Select),
					Col<Person>("*"),
					Kw(KeywordNode.From),
					Tbl<Person>(),
					Kw(KeywordNode.Where),
					BinExp(Null(), BinaryOperation.NotEqual, Col<Person>("Age")));
			var result = SqlifySeq(AnsiSql.Dialect, stream);

			result.Should().Be("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NOT NULL");
		}
	}
}
