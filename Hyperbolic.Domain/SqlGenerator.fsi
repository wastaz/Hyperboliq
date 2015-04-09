namespace Hyperboliq.Domain

module SqlGenerator =
    open Stream
    open Types

    val Sqlify : ISqlDialect -> SqlStream -> string
    val SqlifySeq : ISqlDialect -> SqlNode seq -> string